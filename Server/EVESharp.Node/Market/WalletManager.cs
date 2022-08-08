using System.Data;
using EVESharp.Database;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Market;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Notifications;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Node.Market;

public class WalletManager : IWalletManager
{
    private NotificationSender  Notifications { get; }
    private IDatabaseConnection Database      { get; }

    public WalletManager (IDatabaseConnection database, NotificationSender notificationSender)
    {
        Database      = database;
        Notifications = notificationSender;
    }

    public IWallet AcquireWallet (int ownerID, int walletKey, bool isCorporation = false)
    {
        // TODO: CHECK PERMISSIONS
        return new Wallet (
            ownerID, walletKey, isCorporation, Database, Notifications, this
        );
    }

    /// <summary>
    /// Creates a transaction record in the wallet without modifying the wallet balance
    /// </summary>
    /// <param name="ownerID">The owner of the wallet</param>
    /// <param name="type">The type of transaction</param>
    /// <param name="characterID">The character that performed the transaction</param>
    /// <param name="otherID">The other character's ID</param>
    /// <param name="typeID">The type of item</param>
    /// <param name="quantity">The amount of items</param>
    /// <param name="amount">The amount of ISK</param>
    /// <param name="stationID">The place where the transaction was recorded</param>
    /// <param name="walletKey">The account key where the transaction was recorded</param>
    public void CreateTransactionRecord (
        int ownerID,   TransactionType type, int characterID, int otherID, int typeID, int quantity, double amount,
        int stationID, int             walletKey
    )
    {
        Database.MktRecordTransaction (ownerID, type, characterID, otherID, typeID, quantity, amount, stationID, walletKey);
    }

    public void CreateJournalForOwner (
        MarketReference reference, int  characterID, int    ownerID1,
        int?            ownerID2,  int? referenceID, double amount, double finalBalance, string reason, int walletKey
    )
    {
        Database.MktCreateJournalEntry (
            reference, characterID, ownerID1, ownerID2, referenceID, amount, finalBalance, reason,
            walletKey
        );
    }

    /// <summary>
    /// Creates a new wallet for the given owner with the specified walletKey
    /// </summary>
    /// <param name="ownerID">The ownerID</param>
    /// <param name="walletKey">The walletKey</param>
    /// <param name="startingBalance">The starting balance of the wallet</param>
    public void CreateWallet (int ownerID, int walletKey, double startingBalance)
    {
        Database.MktWalletCreate (startingBalance, ownerID, walletKey);
    }
    
    /// <summary>
    /// Obtains the wallet balance for the given owner and wallet
    /// </summary>
    /// <param name="ownerID">The owner to get the wallet balance for</param>
    /// <param name="walletKey">The wallet to get balance for</param>
    /// <returns>The wallet's balance</returns>
    public double GetWalletBalance (int ownerID, int walletKey = WalletKeys.MAIN)
    {
        return Database.MktWalletGetBalance (walletKey, ownerID);
    }
}