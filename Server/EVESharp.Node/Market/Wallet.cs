using System;
using System.Data;
using EVESharp.Database;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Market;
using EVESharp.Node.Client.Notifications.Wallet;
using EVESharp.Node.Notifications;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Node.Market;

public class Wallet : IWallet
{
    private IDbConnection       Connection;
    public  int                 OwnerID         { get; }
    public  int                 WalletKey       { get; }
    public  double              Balance         { get; set; }
    public  double              OriginalBalance { get; }
    public  IDatabaseConnection Database        { get; }
    public  NotificationSender  Notifications   { get; }
    public  bool                ForCorporation  { get; }
    public  IWalletManager      WalletManager   { get; }

    public Wallet (int ownerID, int walletKey, bool isCorporation, IDatabaseConnection database, NotificationSender notificationSender, IWalletManager walletManager)
    {
        // set some data first
        this.OwnerID        = ownerID;
        this.WalletKey      = walletKey;
        this.ForCorporation = isCorporation;
        this.Database       = database;
        this.Notifications  = notificationSender;
        this.WalletManager  = walletManager;
        
        // obtain exclusive control over the wallet
        Database.GetLock (ref this.Connection, this.GenerateLockName ());
        // also fetch the balance
        this.Balance = this.OriginalBalance = Database.MktWalletGetBalance (ref this.Connection, WalletKey, OwnerID);
    }

    /// <summary>
    /// Generates the lock name used by the database
    /// </summary>
    /// <returns></returns>
    private string GenerateLockName ()
    {
        return $"wallet_{this.OwnerID}_{this.WalletKey}";
    }

    public void Dispose ()
    {
        // if the balance changed, update the record in the database
        if (Math.Abs (Balance - OriginalBalance) > 0.01)
        {
            Database.MktWalletSetBalance (ref this.Connection, Balance, OwnerID, WalletKey);

            if (ForCorporation == false)
            {
                // send notification to the client
                Notifications.NotifyOwner (
                    OwnerID,
                    new OnAccountChange (WalletKey, OwnerID, Balance)
                );
            }
            else
            {
                long corpRoles = (long) CorporationRole.Accountant | (long) CorporationRole.JuniorAccountant;

                corpRoles |= (long) (WalletKey switch
                {
                    WalletKeys.MAIN    => CorporationRole.AccountCanQuery1,
                    WalletKeys.SECOND  => CorporationRole.AccountCanQuery2,
                    WalletKeys.THIRD   => CorporationRole.AccountCanQuery3,
                    WalletKeys.FOURTH  => CorporationRole.AccountCanQuery4,
                    WalletKeys.FIFTH   => CorporationRole.AccountCanQuery5,
                    WalletKeys.SIXTH   => CorporationRole.AccountCanQuery6,
                    WalletKeys.SEVENTH => CorporationRole.AccountCanQuery7,
                    _            => CorporationRole.JuniorAccountant
                });

                Notifications.NotifyCorporationByRole (
                    OwnerID, corpRoles,
                    new OnAccountChange (WalletKey, OwnerID, Balance)
                );
            }
        }

        Database.ReleaseLock (this.Connection, this.GenerateLockName ());
        this.Connection?.Dispose ();
    }

    /// <summary>
    /// Checks that the wallet has enough balance to perform whatever operations
    /// </summary>
    /// <param name="required">The amount required</param>
    /// <exception cref="NotEnoughMoney"></exception>
    public void EnsureEnoughBalance (double required)
    {
        if (Balance < required)
            throw new NotEnoughMoney (Balance, required);
    }

    /// <summary>
    /// Creates a journal record for the given wallet and adds to the balance
    /// </summary>
    /// <param name="reference">The type of market reference</param>
    /// <param name="ownerID1">Character involved</param>
    /// <param name="ownerID2">Other character involved</param>
    /// <param name="referenceID"></param>
    /// <param name="amount">The amount of ISK to add</param>
    /// <param name="reason">Extra information for the EVE Client</param>
    public void CreateJournalRecord (MarketReference reference, int ownerID1, int? ownerID2, int? referenceID, double amount, string reason = "")
    {
        // subtract balance
        Balance += amount;

        // create journal entry
        WalletManager.CreateJournalForOwner (
            reference, OwnerID, ownerID1, ownerID2, referenceID, amount,
            Balance, reason, WalletKey
        );
    }

    /// <summary>
    /// Creates a journal record for the given wallet and adds to the balance
    /// </summary>
    /// <param name="reference">The type of market reference</param>
    /// <param name="ownerID2">Other character involved</param>
    /// <param name="referenceID"></param>
    /// <param name="amount">The amount of ISK to add</param>
    /// <param name="reason">Extra information for the EVE Client</param>
    public void CreateJournalRecord (MarketReference reference, int? ownerID2, int? referenceID, double amount, string reason = "")
    {
        // subtract balance
        Balance += amount;

        // create journal entry
        WalletManager.CreateJournalForOwner (
            reference, OwnerID, OwnerID, ownerID2, referenceID, amount,
            Balance, reason, WalletKey
        );
    }

    /// <summary>
    /// Creates a transaction record in the wallet modifying the wallet balance
    /// </summary>
    /// <param name="type">The type of transaction</param>
    /// <param name="characterID">The character performing the transaction</param>
    /// <param name="otherID">The other character's ID</param>
    /// <param name="typeID">The type of item</param>
    /// <param name="quantity">The amount of items</param>
    /// <param name="amount">The amount of ISK</param>
    /// <param name="stationID">The place where the transaction was recorded</param>
    public void CreateTransactionRecord (TransactionType type, int characterID, int otherID, int typeID, int quantity, double amount, int stationID)
    {
        Balance += amount;
        // market transactions do not affect the wallet value because these are paid either when placing the sell/buy order
        // or when fullfiling it
        WalletManager.CreateTransactionRecord (
            OwnerID, type, characterID, otherID, typeID, quantity, amount, stationID,
            WalletKey
        );
    }
}