using System.Data;
using System.Runtime.CompilerServices;
using EVESharp.Database;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Market;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Wallet;
using EVESharp.Node.Data.Inventory;
using EVESharp.Node.Market;
using EVESharp.Node.Unit.Utils;
using EVESharp.PythonTypes.Types.Database;
using HarmonyLib;
using Moq;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace EVESharp.Node.Unit.MarketTests;

[TestFixture]
public class WalletTests
{
    private Mock <IDatabaseConnection> mDatabaseMock           = Utils.Database.DatabaseLockMocked ();
    private Mock <INotificationSender> mNotificationSenderMock = new Mock <INotificationSender> ();

    private Wallets mWallets;
    private Harmony mHarmony;

    private static double sCurrentBalance;
    private static int    sCurrentOwnerID;
    private static int    sCurrentWalletKey;
    
    [HarmonyPatch(typeof(WalletDB), nameof(WalletDB.MktWalletGetBalance))]
    static bool MktWalletGetBalance (IDatabaseConnection Database, ref IDbConnection connection, int walletKey, int ownerID, ref double __result)
    {
        __result = sCurrentBalance;
        
        return false;
    }

    [HarmonyPatch(typeof(WalletDB), nameof(WalletDB.MktWalletSetBalance))]
    static bool MktWalletSetBalance (IDatabaseConnection Database, ref IDbConnection connection, double balance, int ownerID, int walletKey)
    {
        return false;
    }
    
    
    [SetUp]
    public void SetUp ()
    {
        this.mHarmony = new Harmony ("WalletTest");
        this.mWallets = new Wallets (this.mDatabaseMock.Object, this.mNotificationSenderMock.Object);

        this.mHarmony.Setup (this);
    }

    [TearDown]
    public void TearDown ()
    {
        this.mHarmony.UnpatchAll ();
    }

    [TestCase(14000, WalletKeys.MAIN,    false, 1500, 100)]
    [TestCase(14000, WalletKeys.MAIN,    false, 1500, 1500)]
    [TestCase(14000, WalletKeys.MAIN,    true,  1500, 100)]
    [TestCase(14000, WalletKeys.SECOND,  true,  1500, 1500)]
    [TestCase(14000, WalletKeys.THIRD,   true,  1500, 100)]
    [TestCase(14000, WalletKeys.FOURTH,  true,  1500, 1500)]
    [TestCase(14000, WalletKeys.FIFTH,   true,  1500, 100)]
    [TestCase(14000, WalletKeys.SIXTH,   true,  1500, 1500)]
    [TestCase(14000, WalletKeys.SEVENTH, true,  1500, 1500)]
    public void WalletTest (int ownerID, int walletKey, bool isCorporation, double balance, double request)
    {
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

        sCurrentBalance = balance;
        
        // obtain the wallet
        using (IWallet wallet = this.mWallets.AcquireWallet (ownerID, walletKey, isCorporation))
        {
            Assert.DoesNotThrow (() => wallet.EnsureEnoughBalance (request));            
        }
        
        this.mDatabaseMock.Verify ();
        this.mDatabaseMock.VerifyNoOtherCalls ();
    }
}