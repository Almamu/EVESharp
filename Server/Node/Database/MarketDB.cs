using System.Collections.Generic;
using Common.Database;
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