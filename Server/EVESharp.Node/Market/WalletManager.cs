using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.EVE.Market;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Corporation;
using EVESharp.EVE.Wallet;
using EVESharp.Node.Notifications;
using MySql.Data.MySqlClient;

namespace EVESharp.Node.Market;

public class WalletManager
{
    private NotificationSender Notifications { get; }
    private DatabaseConnection Database      { get; }

    public WalletManager (DatabaseConnection database, NotificationSender notificationSender)
    {
        Database      = database;
        Notifications = notificationSender;
    }

    public Wallet AcquireWallet (int ownerID, int walletKey, bool isCorporation = false)
    {
        // TODO: CHECK PERMISSIONS
        return new Wallet
        {
            Connection      = this.AcquireLock (ownerID, walletKey, out double balance),
            OwnerID         = ownerID,
            WalletKey       = walletKey,
            Balance         = balance,
            OriginalBalance = balance,
            Database        = Database,
            Notifications   = Notifications,
            ForCorporation  = isCorporation,
            WalletManager   = this
        };
    }

    /// <summary>
    /// Acquires a lock for modifying wallets on the database
    /// </summary>
    /// <returns></returns>
    private MySqlConnection AcquireLock (int ownerID, int walletKey, out double balance)
    {
        MySqlConnection connection = null;
        // acquire the lock
        Database.GetLock (ref connection, $"wallet_{ownerID}_{walletKey}");

        // get the current owner's balance
        balance = Database.MktWalletGetBalance (ref connection, walletKey, ownerID);

        return connection;
    }

    /// <summary>
    /// Special situation for Market
    ///
    /// Frees the table lock for the orders table
    /// This allows to take exclusive control over it and perform any actions required
    /// </summary>
    /// <param name="connection"></param>
    public void ReleaseLock (MySqlConnection connection, int ownerID, int walletKey)
    {
        Database.ReleaseLock (connection, $"wallet_{ownerID}_{walletKey}");
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

    public bool IsAccessAllowed (Session session, int accountKey, int ownerID)
    {
        if (ownerID == session.CharacterID)
            return true;

        if (ownerID == session.CorporationID)
        {
            // check for permissions
            // check if the character has any accounting roles and set the correct accountKey based on the data
            if (CorporationRole.AccountCanQuery1.Is (session.CorporationRole) && accountKey == Keys.MAIN)
                return true;
            if (CorporationRole.AccountCanQuery2.Is (session.CorporationRole) && accountKey == Keys.SECOND)
                return true;
            if (CorporationRole.AccountCanQuery3.Is (session.CorporationRole) && accountKey == Keys.THIRD)
                return true;
            if (CorporationRole.AccountCanQuery4.Is (session.CorporationRole) && accountKey == Keys.FOURTH)
                return true;
            if (CorporationRole.AccountCanQuery5.Is (session.CorporationRole) && accountKey == Keys.FIFTH)
                return true;
            if (CorporationRole.AccountCanQuery6.Is (session.CorporationRole) && accountKey == Keys.SIXTH)
                return true;
            if (CorporationRole.AccountCanQuery7.Is (session.CorporationRole) && accountKey == Keys.SEVENTH)
                return true;

            // last chance, accountant role
            if (CorporationRole.Accountant.Is (session.CorporationRole))
                return true;
            if (CorporationRole.JuniorAccountant.Is (session.CorporationRole))
                return true;
        }

        return false;
    }

    public bool IsTakeAllowed (Session session, int accountKey, int ownerID)
    {
        if (ownerID == session.CharacterID)
            return true;

        if (ownerID == session.CorporationID)
        {
            // check for permissions
            // check if the character has any accounting roles and set the correct accountKey based on the data
            if (CorporationRole.AccountCanTake1.Is (session.CorporationRole) && accountKey == Keys.MAIN)
                return true;
            if (CorporationRole.AccountCanTake2.Is (session.CorporationRole) && accountKey == Keys.SECOND)
                return true;
            if (CorporationRole.AccountCanTake3.Is (session.CorporationRole) && accountKey == Keys.THIRD)
                return true;
            if (CorporationRole.AccountCanTake4.Is (session.CorporationRole) && accountKey == Keys.FOURTH)
                return true;
            if (CorporationRole.AccountCanTake5.Is (session.CorporationRole) && accountKey == Keys.FIFTH)
                return true;
            if (CorporationRole.AccountCanTake6.Is (session.CorporationRole) && accountKey == Keys.SIXTH)
                return true;
            if (CorporationRole.AccountCanTake7.Is (session.CorporationRole) && accountKey == Keys.SEVENTH)
                return true;
            if (CorporationRole.Accountant.Is (session.CorporationRole))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Obtains the wallet balance for the given owner and wallet
    /// </summary>
    /// <param name="ownerID">The owner to get the wallet balance for</param>
    /// <param name="walletKey">The wallet to get balance for</param>
    /// <returns>The wallet's balance</returns>
    public double GetWalletBalance (int ownerID, int walletKey = Keys.MAIN)
    {
        return Database.MktWalletGetBalance (walletKey, ownerID);
    }
}