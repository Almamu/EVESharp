using System;
using MySql.Data.MySqlClient;
using Node.Database;
using Node.Exceptions;
using Node.Network;
using Node.Notifications.Client.Character;

namespace Node.Market
{
    public class Wallet : IDisposable
    {
        public int OwnerID { get; init; }
        public int WalletKey { get; init; }
        public MySqlConnection Connection { get; init; }
        public double Balance { get; set; }
        public double OriginalBalance { get; init; }
        public WalletDB DB { get; init; }
        public NotificationManager NotificationManager { get; init; }
        
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
            this.DB.CreateJournalForCharacter(
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
            this.DB.CreateJournalForCharacter(
                reference, this.OwnerID, this.OwnerID, ownerID2, referenceID, amount,
                this.Balance, reason, this.WalletKey
            );
        }

        /// <summary>
        /// Creates a transaction record in the wallet modifying the wallet balance
        /// </summary>
        /// <param name="type">The type of transaction</param>
        /// <param name="otherID">The other character's ID</param>
        /// <param name="typeID">The type of item</param>
        /// <param name="quantity">The amount of items</param>
        /// <param name="amount">The amount of ISK</param>
        /// <param name="stationID">The place where the transaction was recorded</param>
        public void CreateTransactionRecord(TransactionType type, int otherID, int typeID, int quantity, double amount, int stationID)
        {
            this.Balance += amount;
            // market transactions do not affect the wallet value because these are paid either when placing the sell/buy order
            // or when fullfiling it
            this.DB.CreateTransactionForCharacter(
                this.OwnerID, otherID, type, typeID, quantity, amount, stationID
            );
        }
        
        public void Dispose()
        {
            // if the balance changed, update the record in the database
            if (Math.Abs(this.Balance - this.OriginalBalance) > 0.01)
            {
                // TODO: PROPERLY SUPPORT WALLET KEYS AND OWNERIDS
                this.DB.SetCharacterBalance(this.Connection, this.OwnerID, this.Balance);
                // send notification to the client
                this.NotificationManager.NotifyCharacter(this.OwnerID, 
                    new OnAccountChange(this.WalletKey, this.OwnerID, this.Balance)
                );
            }

            this.DB.ReleaseLock(this.Connection, this.OwnerID, this.WalletKey);
            this.Connection?.Dispose();
        }
    }
}