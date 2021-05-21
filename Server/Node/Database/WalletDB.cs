using System;
using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Market;
using PythonTypes.Types.Collections;

namespace Node.Database
{
    public class WalletDB : DatabaseAccessor
    {
        public void CreateJournalForOwner(MarketReference reference, int characterID, int ownerID1,
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

        public void CreateTransactionForOwner(int entityID, int characterID, int? clientID, TransactionType sellBuy,
            int typeID, int quantity, double price, int stationID, int accountKey)
        {
            Database.PrepareQuery(
                "INSERT INTO mktTransactions(transactionDateTime, typeID, quantity, price, transactionType, characterID, clientID, stationID, accountKey, entityID)VALUE(@transactionDateTime, @typeID, @quantity, @price, @transactionType, @characterID, @clientID, @stationID, @accountKey, @entityID)",
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
                    {"@accountKey", accountKey},
                    {"@entityID", entityID}
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
            balance = this.GetWalletBalance(ref connection, ownerID, walletKey);
            
            return connection;
        }

        public double GetWalletBalance(ref MySqlConnection connection, int ownerID, int walletKey)
        {
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT balance FROM mktWallets WHERE `key` = @walletKey AND ownerID = @ownerID",
                new Dictionary<string, object>()
                {
                    {"@walletKey", walletKey},
                    {"@ownerID", ownerID}
                }
            );

            using (reader)
            {
                if (reader.Read() == false)
                    return 0.0;

                return reader.GetDouble(0);
            }
        }

        public double GetWalletBalance(int ownerID, int walletKey)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT balance FROM mktWallets WHERE `key` = @walletKey AND ownerID = @ownerID",
                new Dictionary<string, object>()
                {
                    {"@walletKey", walletKey},
                    {"@ownerID", ownerID}
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

        public double GetCharacterBalance(ref MySqlConnection connection, int characterID)
        {
            return this.GetWalletBalance(ref connection, characterID, 1000);
        }

        public double GetCharacterBalance(int characterID)
        {
            return this.GetWalletBalance(characterID, 1000);
        }

        public void SetWalletBalance(int accountKey, int ownerID, double balance)
        {
            Database.PrepareQuery(
                "REPLACE INTO mktWallets (`key`, ownerID, balance)VALUE(@accountKey, @ownerID, @balance)",
                new Dictionary<string, object>()
                {
                    {"@balance", balance},
                    {"@ownerID", ownerID},
                    {"@accountKey", accountKey}
                }
            );
        }

        public void SetWalletBalance(MySqlConnection connection, int accountKey, int ownerID, double balance)
        {
            Database.PrepareQuery(ref connection,
                "REPLACE INTO mktWallets (`key`, ownerID, balance)VALUE(@accountKey, @ownerID, @balance)",
                new Dictionary<string, object>()
                {
                    {"@balance", balance},
                    {"@ownerID", ownerID},
                    {"@accountKey", accountKey}
                }
            ).Close();
        }
        
        public void SetCharacterBalance(int characterID, double balance)
        {
            this.SetWalletBalance(1000, characterID, balance);
        }

        public void SetCharacterBalance(MySqlConnection connection, int characterID, double balance)
        {
            this.SetWalletBalance(connection, 1000, characterID, balance);
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
        }

        /// <summary>
        /// Creates a new wallet for the given <paramref name="ownerID"/> with the given <paramref name="walletKey"/>
        /// </summary>
        /// <param name="ownerID"></param>
        /// <param name="walletKey"></param>
        /// <param name="startBalance"></param>
        public void CreateWallet(int ownerID, int walletKey, double startBalance)
        {
            Database.PrepareQuery(
                "INSERT INTO mktWallets(`key`, ownerID, balance)VALUES(@walletKey, @ownerID, @balance)",
                new Dictionary<string, object>()
                {
                    {"@walletKey", walletKey},
                    {"@ownerID", ownerID},
                    {"@balance", startBalance}
                }
            );
        }

        public PyList GetWalletDivisionsForOwner(int ownerID, List<int> walletKeys)
        {
            return Database.PreparePackedRowListQuery(
                $"SELECT `key`, balance FROM mktWallets WHERE ownerID = @ownerID AND `key` IN ({string.Join(',', walletKeys)})",
                new Dictionary<string, object>()
                {
                    {"@ownerID", ownerID}
                }
            );
        }
        
        public WalletDB(DatabaseConnection db) : base(db)
        {
        }
    }
}