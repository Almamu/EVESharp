using System;
using System.Collections.Generic;
using Common.Database;
using Node.Market;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public enum TransactionType
    {
        Sell = 0,
        Buy = 1,
        Either = 2,
    }
    
    public class MarketDB : DatabaseAccessor
    {
        public MarketDB(DatabaseConnection db) : base(db)
        {
        }

        public void CreateJournalForCharacter(MarketReference reference, int ownerID1,
            int? ownerID2, int? referenceID, double amount, double balance, string reason, int accountKey)
        {
            reason = reason.Substring(0, Math.Min(reason.Length, 43));
            
            Database.PrepareQuery(
                "INSERT INTO market_journal(transactionDate, entryTypeID, ownerID1, ownerID2, referenceID, amount, balance, description, accountKey)VALUES(@transactionDate, @entryTypeID, @ownerID1, @ownerID2, @referenceID, @amount, @balance, @description, @accountKey)",
                new Dictionary<string, object>()
                {
                    {"@transactionDate", DateTime.UtcNow.ToFileTimeUtc()},
                    {"@entryTypeID", (int) reference},
                    {"@ownerID1", ownerID1},
                    {"@ownerID2", ownerID2},
                    {"@referenceID", referenceID},
                    {"@amount", amount},
                    {"@balance", balance},
                    {"@description", reason},
                    {"@accountKey", accountKey}
                }
            );
        }

        public void CreateTransactionForCharacter(int characterID, int? clientID, TransactionType sellBuy,
            int typeID, int quantity, double price, int stationID, int regionID, bool corpTransaction = false)
        {
            Database.PrepareQuery(
                "INSERT INTO market_transactions(transactionDateTime, typeID, quantity, price, transactionType, characterID, clientID, regionID, stationID, corpTransaction)VALUE(@transactionDateTime, @typeID, @quantity, @price, @transactionType, @characterID, @clientID, @regionID, @stationID, @corpTransaction)",
                new Dictionary<string, object>()
                {
                    {"@transactionDateTime", DateTime.UtcNow.ToFileTimeUtc()},
                    {"@typeID", typeID},
                    {"@quantity", quantity},
                    {"@price", price},
                    {"@transactionType", (int) sellBuy},
                    {"@characterID", characterID},
                    {"@clientID", clientID},
                    {"@regionID", regionID},
                    {"@stationID", stationID},
                    {"@corpTransaction", corpTransaction}
                }
            );
        }

        public PyDataType CharGetNewTransactions(int characterID, int? clientID, TransactionType sellBuy, int? typeID, int quantity, int minPrice)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"@characterID", characterID},
                {"@quantity", quantity},
                {"@price", minPrice}
            };
            
            string query =
                "SELECT transactionID, transactionDateTime, typeID, quantity, price, transactionType," +
                "0 AS corpTransaction, clientID, stationID " +
                "FROM market_transactions WHERE characterID=@characterID AND quantity >= @quantity AND price >= @price";

            if (sellBuy != TransactionType.Either)
            {
                query += " AND transactionType=@transactionType";
                parameters["@transactionType"] = (int) sellBuy;
            }

            if (typeID != null)
            {
                query += " AND typeID=@typeID";
                parameters["@typeID"] = (int) typeID;
            }

            if (clientID != null)
            {
                query += " AND clientID=@clientID";
                parameters["@clientID"] = (int) clientID;
            }
            
            return Database.PrepareRowsetQuery(query, parameters);
        }

        public PyDataType GetCharOrders(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT" +
                " orderID, typeID, charID, regionID, stationID, `range`, bid, price, volEntered, volRemaining," +
                " issued, orderState, minVolume, contraband, accountID, duration, isCorp, solarSystemID, escrow " +
                "FROM market_orders " +
                "WHERE charID=@characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }
    }
}