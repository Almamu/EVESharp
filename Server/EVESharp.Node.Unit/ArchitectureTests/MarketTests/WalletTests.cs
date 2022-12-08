using EVESharp.Database;
using EVESharp.Database.Corporations;
using EVESharp.Database.Inventory;
using EVESharp.Database.Market;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Market;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Wallet;
using EVESharp.Node.Market;
using Moq;
using NUnit.Framework;

namespace EVESharp.Node.Unit.ArchitectureTests.MarketTests;

[TestFixture]
public class WalletTests
{
    private Mock <IDatabase> mDatabaseMock           = Utils.Database.DatabaseLockMocked ();
    private Mock <INotificationSender> mNotificationSenderMock = new Mock <INotificationSender> ();

    private Wallets mWallets;

    [SetUp]
    public void SetUp ()
    {
        this.mWallets = new Wallets (this.mDatabaseMock.Object, this.mNotificationSenderMock.Object);
    }

    [TestCase(ItemRanges.UserGenerated.MIN,     WalletKeys.MAIN,   false, 1500, 100,  1400)]
    [TestCase(ItemRanges.UserGenerated.MIN + 1, WalletKeys.MAIN,   false, 1500, 1500, 0)]
    [TestCase(ItemRanges.UserGenerated.MIN + 2, WalletKeys.MAIN,   true,  1500, 100,  1400)]
    [TestCase(ItemRanges.UserGenerated.MIN + 3, WalletKeys.SECOND, true,  1500, 1500, 0)]
    [TestCase(ItemRanges.UserGenerated.MIN + 4, WalletKeys.THIRD,  true,  1500, 100,  1400)]
    [TestCase(ItemRanges.UserGenerated.MIN + 5, WalletKeys.FOURTH, true,  1500, 1500, 0)]
    [TestCase(ItemRanges.UserGenerated.MIN + 6, WalletKeys.FIFTH,  true,  1500, 100,  1400)]
    [TestCase(ItemRanges.UserGenerated.MIN + 7, WalletKeys.SIXTH,  true,  1500, 1500, 0)]
    [TestCase(ItemRanges.UserGenerated.MIN + 8, WalletKeys.SEVENTH, true,  1500, 1500, 0)]
    public void WalletTest (int ownerID, int walletKey, bool isCorporation, double balance, double request, double expectedBalance)
    {
        // first create the wallet
        this.mWallets.CreateWallet (ownerID, walletKey, balance);
        
        if (isCorporation == false)
        {
            this.mNotificationSenderMock.Setup (x => x.NotifyOwner (
                It.Is<int>(x => x == ownerID),
                It.Is<OnAccountChange> (x => x.AccountKey == walletKey && x.OwnerID == ownerID)
            ));
        }
        else
        {
            // TODO: VALIDATE CORPROLE FOR DIFFERENT WALLET KEYS
            this.mNotificationSenderMock.Setup (x => x.NotifyCorporationByRole (
                It.Is<int>(x => x == ownerID),
                It.Is<long>(x => (x & (long) CorporationRole.Accountant) != 0 && (x & (long) CorporationRole.JuniorAccountant) != 0),
                It.Is<OnAccountChange> (x => x.AccountKey == walletKey && x.OwnerID == ownerID)
            ));    
        }

        // obtain the wallet
        using (IWallet wallet = this.mWallets.AcquireWallet (ownerID, walletKey, isCorporation))
        {
            Assert.AreEqual (ownerID,   wallet.OwnerID);
            Assert.AreEqual (walletKey, wallet.WalletKey);
            
            Assert.DoesNotThrow (() => wallet.EnsureEnoughBalance (request));     
            wallet.CreateJournalRecord (MarketReference.Bounty, 0, null, null, -request);
            
            Assert.AreEqual (wallet.Balance, expectedBalance);
        }
        
        this.mDatabaseMock.Verify ();
        this.mDatabaseMock.VerifyNoOtherCalls ();
    }

    [TestCase(ItemRanges.UserGenerated.MIN + 9,  1500, 150000)]
    [TestCase(ItemRanges.UserGenerated.MIN + 10, 1500, 1500.1)]
    [TestCase(ItemRanges.UserGenerated.MIN + 11, 1500, 1500.01)]
    public void WalletErrorTest (int ownerID, double balance, double request)
    {
        // first create the wallet
        this.mWallets.CreateWallet (ownerID, WalletKeys.MAIN, balance);

        using (IWallet wallet = this.mWallets.AcquireWallet (ownerID, WalletKeys.MAIN))
        {
            Assert.Throws <NotEnoughMoney> (() => wallet.EnsureEnoughBalance (request));
        }
    }
}