using System;
using System.Data;
using EVESharp.Database;
using EVESharp.EVE.Client.Exceptions;
using EVESharp.EVE.Market;
using EVESharp.EVE.StaticData.Corporation;
using EVESharp.EVE.Wallet;
using EVESharp.Node.Client.Notifications.Wallet;
using EVESharp.Node.Notifications;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Node.Market;

public class Wallet : IDisposable
{
    public IDbConnection       Connection;
    public int                 OwnerID         { get; init; }
    public int                 WalletKey       { get; init; }
    public double              Balance         { get; set; }
    public double              OriginalBalance { get; init; }
    public IDatabaseConnection Database        { get; init; }
    public NotificationSender  Notifications   { get; init; }
    public bool                ForCorporation  { get; init; }
    public WalletManager       WalletManager   { get; init; }

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
                    Keys.MAIN    => CorporationRole.AccountCanQuery1,
                    Keys.SECOND  => CorporationRole.AccountCanQuery2,
                    Keys.THIRD   => CorporationRole.AccountCanQuery3,
                    Keys.FOURTH  => CorporationRole.AccountCanQuery4,
                    Keys.FIFTH   => CorporationRole.AccountCanQuery5,
                    Keys.SIXTH   => CorporationRole.AccountCanQuery6,
                    Keys.SEVENTH => CorporationRole.AccountCanQuery7,
                    _            => CorporationRole.JuniorAccountant
                });

                Notifications.NotifyCorporationByRole (
                    OwnerID, corpRoles,
                    new OnAccountChange (WalletKey, OwnerID, Balance)
                );
            }
        }

        WalletManager.ReleaseLock (this.Connection, OwnerID, WalletKey);
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