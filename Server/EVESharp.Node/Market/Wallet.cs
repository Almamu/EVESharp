using System;
using System.Collections.Generic;
using System.Diagnostics;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.EVE;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Wallet;
using EVESharp.Node.StaticData.Corporation;
using MySql.Data.MySqlClient;
using EVESharp.Node.Notifications.Client.Character;

namespace EVESharp.Node.Market;

public class Wallet : IDisposable
{
    public int                 OwnerID   { get; init; }
    public int                 WalletKey { get; init; }
    public MySqlConnection     Connection;
    public double              Balance             { get; set; }
    public double              OriginalBalance     { get; init; }
    public DatabaseConnection  Database            { get; init; }
    public NotificationManager NotificationManager { get; init; }
    public bool                ForCorporation      { get; init; }
    public WalletManager       WalletManager       { get; init; }
        
    /// <summary>
    /// Checks that the wallet has enough balance to perform whatever operations
    /// </summary>
    /// <param name="required">The amount required</param>
    /// <exception cref="NotEnoughMoney"></exception>
    public void EnsureEnoughBalance(double required)
    {
        if (this.Balance < required)
            throw new NotEnoughMoney(this.Balance, required);
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
    public void CreateJournalRecord(MarketReference reference, int ownerID1, int? ownerID2, int? referenceID, double amount, string reason = "")
    {
        // subtract balance
        this.Balance += amount;
            
        // create journal entry
        this.WalletManager.CreateJournalForOwner(
            reference, this.OwnerID, ownerID1, ownerID2, referenceID, amount,
            this.Balance, reason, this.WalletKey
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
    public void CreateJournalRecord(MarketReference reference, int? ownerID2, int? referenceID, double amount, string reason = "")
    {
        // subtract balance
        this.Balance += amount;
            
        // create journal entry
        this.WalletManager.CreateJournalForOwner(
            reference, this.OwnerID, this.OwnerID, ownerID2, referenceID, amount,
            this.Balance, reason, this.WalletKey
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
    public void CreateTransactionRecord(TransactionType type, int characterID, int otherID, int typeID, int quantity, double amount, int stationID)
    {
        this.Balance += amount;
        // market transactions do not affect the wallet value because these are paid either when placing the sell/buy order
        // or when fullfiling it
        this.WalletManager.CreateTransactionRecord(this.OwnerID, type, characterID, otherID, typeID, quantity, amount, stationID, this.WalletKey);
    }
        
    public void Dispose()
    {
        // if the balance changed, update the record in the database
        if (Math.Abs(this.Balance - this.OriginalBalance) > 0.01)
        {
            Database.Procedure(
                ref this.Connection,
                WalletDB.SET_WALLET_BALANCE,
                new Dictionary<string, object>()
                {
                    {"_balance", this.Balance},
                    {"_ownerID", this.OwnerID},
                    {"_walletKey", this.WalletKey}
                }
            );

            if (this.ForCorporation == false)
            {
                // send notification to the client
                this.NotificationManager.NotifyOwner(this.OwnerID, 
                                                     new OnAccountChange(this.WalletKey, this.OwnerID, this.Balance)
                );
            }
            else
            {
                long corpRoles = (long) CorporationRole.Accountant | (long) CorporationRole.JuniorAccountant;

                corpRoles |= (long) (this.WalletKey switch
                {
                    WalletKeys.MAIN_WALLET    => CorporationRole.AccountCanQuery1,
                    WalletKeys.SECOND_WALLET  => CorporationRole.AccountCanQuery2,
                    WalletKeys.THIRD_WALLET   => CorporationRole.AccountCanQuery3,
                    WalletKeys.FOURTH_WALLET  => CorporationRole.AccountCanQuery4,
                    WalletKeys.FIFTH_WALLET   => CorporationRole.AccountCanQuery5,
                    WalletKeys.SIXTH_WALLET   => CorporationRole.AccountCanQuery6,
                    WalletKeys.SEVENTH_WALLET => CorporationRole.AccountCanQuery7,
                    _                         => CorporationRole.JuniorAccountant
                });
                    
                this.NotificationManager.NotifyCorporationByRole(
                    this.OwnerID, corpRoles,
                    new OnAccountChange(this.WalletKey, this.OwnerID, this.Balance)
                );
            }
        }

        this.WalletManager.ReleaseLock(this.Connection, this.OwnerID, this.WalletKey);
        this.Connection?.Dispose();
    }
}