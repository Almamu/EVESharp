using System;
using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Market;

namespace Node.Database
{
    public class WalletDB : DatabaseAccessor
    {
        public void CreateJournalForCharacter(MarketReference reference, int characterID, int ownerID1,
            int? ownerID2, int? referenceID, double amount, double finalBalance, string reason, int accountKey)
        {
            reason = reason.Substring(0, Math.Min(reason.Length, 43));
            
            Database.PrepareQuery(
                "INSERT INTO market_journal(transactionDate, entryTypeID, charID, ownerID1, ownerID2, referenceID, amount, balance, description, accountKey)VALUES(@transactionDate, @entryTypeID, @charID, @ownerID1, @ownerID2, @referenceID, @amount, @balance, @description, @accountKey)",
                new Dictionary<string, object>()
                {
                    {"@transactionDate", DateTime.UtcNow.ToFileTimeUtc()},
                    {"@entryTypeID", (int) reference},
                    {"@charID", characterID},
                    {"@ownerID1", ownerID1},
                    {"@ownerID2", ownerID2},
                    {"@referenceID", referenceID},
                    {"@amount", amount},
                    {"@balance", finalBalance},
                    {"@description", reason},
                    {"@accountKey", accountKey}
                }
            );
        }

        public void CreateTransactionForCharacter(int characterID, int? clientID, TransactionType sellBuy,
            int typeID, int quantity, double price, int stationID, bool corpTransaction = false)
        {
            Database.PrepareQuery(
                "INSERT INTO mktTransactions(transactionDateTime, typeID, quantity, price, transactionType, characterID, clientID, stationID, corpTransaction)VALUE(@transactionDateTime, @typeID, @quantity, @price, @transactionType, @characterID, @clientID, @stationID, @corpTransaction)",
                new Dictionary<string, object>()
                {
                    {"@transactionDateTime", DateTime.UtcNow.ToFileTimeUtc()},
                    {"@typeID", typeID},
                    {"@quantity", quantity},
                    {"@price", price},
                    {"@transactionType", (int) sellBuy},
                    {"@characterID", characterID},
                    {"@clientID", clientID},
                    {"@stationID", stationID},
                    {"@corpTransaction", corpTransaction}
                }
            );
        }

        /// <summary>
        /// Special situation for wallets
        ///
        /// Acquires a lock for modifying wallets on the database
        /// </summary>
        /// <returns></returns>
        public MySqlConnection AcquireLock(int ownerID, int walletKey)
        {
            MySqlConnection connection = null;
            // acquire the lock
            Database.GetLock(ref connection, $"wallet_{ownerID}_{walletKey}");

            return connection;
        }

        /// <summary>
        /// Special situation for wallets
        ///
        /// Acquires a lock for modifying wallets on the database
        /// </summary>
        /// <returns></returns>
        public MySqlConnection AcquireLock(int ownerID, int walletKey, out double balance)
        {
            MySqlConnection connection = null;
            // acquire the lock
            Database.GetLock(ref connection, $"wallet_{ownerID}_{walletKey}");

            // get the current owner's balance
            // TODO: SUPPORT CORPS AND WALLET KEYS!!
            balance = this.GetCharacterBalance(ref connection, ownerID);
            
            return connection;
        }

        public double GetCharacterBalance(ref MySqlConnection connection, int characterID)
        {
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT balance FROM chrInformation WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
            
            using (reader)
            {
                if (reader.Read() == false)
                    return 0.0;

                return reader.GetDouble(0);
            }
        }

        public double GetCharacterBalance(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT balance FROM chrInformation WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return 0.0;

                return reader.GetDouble(0);
            }
        }

        public void SetCharacterBalance(int characterID, double balance)
        {
            Database.PrepareQuery("UPDATE chrInformation SET balance = @balance WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@balance", balance},
                    {"@characterID", characterID}
                }
            );
        }

        public void SetCharacterBalance(MySqlConnection connection, int characterID, double balance)
        {
            Database.PrepareQuery(ref connection,
                "UPDATE chrInformation SET balance = @balance WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@balance", balance},
                    {"@characterID", characterID}
                }
            ).Close();
        }

        /// <summary>
        /// Special situation for Market
        ///
        /// Frees the table lock for the orders table
        /// This allows to take exclusive control over it and perform any actions required
        /// </summary>
        /// <param name="connection"></param>
        public void ReleaseLock(MySqlConnection connection, int ownerID, int walletKey)
        {
            Database.ReleaseLock(connection, $"wallet_{ownerID}_{walletKey}");
            // close the connection
            connection.Close();
        }
        
        public WalletDB(DatabaseConnection db) : base(db)
        {
        }
    }
}