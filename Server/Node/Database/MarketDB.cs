using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Market;
using PythonTypes.Types.Database;
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
                "INSERT INTO mktTransactions(transactionDateTime, typeID, quantity, price, transactionType, characterID, clientID, regionID, stationID, corpTransaction)VALUE(@transactionDateTime, @typeID, @quantity, @price, @transactionType, @characterID, @clientID, @regionID, @stationID, @corpTransaction)",
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
                "FROM mktTransactions WHERE characterID=@characterID AND quantity >= @quantity AND price >= @price";

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
                "FROM mktOrders " +
                "WHERE charID=@characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public PyDataType GetStationAsks(int stationID)
        {
            return Database.PrepareIntRowDictionary(
                "SELECT typeID, MAX(price) AS price, volRemaining, stationID FROM mktOrders WHERE stationID = @stationID GROUP BY typeID", 0,
                new Dictionary<string, object>()
                {
                    {"@stationID", stationID}
                }
            );
        }

        public PyDataType GetSystemAsks(int solarSystemID)
        {
            return Database.PrepareIntRowDictionary(
                "SELECT typeID, MAX(price) AS price, volRemaining, stationID FROM mktOrders WHERE solarSystemID = @solarSystemID GROUP BY typeID", 0,
                new Dictionary<string, object>()
                {
                    {"@solarSystemID", solarSystemID}
                }
            );
        }

        public PyDataType GetRegionBest(int regionID)
        {
            return Database.PrepareIntRowDictionary(
                "SELECT typeID, MAX(price) AS price, volRemaining, stationID FROM mktOrders WHERE regionID = @regionID GROUP BY typeID", 0,
                new Dictionary<string, object>()
                {
                    {"@regionID", regionID}
                }
            );
        }

        public PyList GetOrders(int regionID, int currentSolarSystem, int typeID)
        {
            return new PyDataType[]
            {
                Database.PrepareCRowsetQuery(
                    "SELECT price, volRemaining, typeID, `range`, orderID, volEntered, minVolume, bid, issued, duration, stationID, regionID, solarSystemID, jumps FROM mktOrders, mapPrecalculatedSolarSystemJumps WHERE mapPrecalculatedSolarSystemJumps.fromSolarSystemID = solarSystemID AND mapPrecalculatedSolarSystemJumps.toSolarSystemID = @currentSolarSystem AND regionID = @regionID AND typeID = @typeID AND bid = @bid",
                    new Dictionary<string, object>()
                    {
                        {"@regionID", regionID},
                        {"@typeID", typeID},
                        {"@bid", TransactionType.Sell},
                        {"@currentSolarSystem", currentSolarSystem}
                    }
                ),
                Database.PrepareCRowsetQuery(
                    "SELECT price, volRemaining, typeID, `range`, orderID, volEntered, minVolume, bid, issued, duration, stationID, regionID, solarSystemID, jumps FROM mktOrders, mapPrecalculatedSolarSystemJumps WHERE mapPrecalculatedSolarSystemJumps.fromSolarSystemID = solarSystemID AND mapPrecalculatedSolarSystemJumps.toSolarSystemID = @currentSolarSystem AND regionID = @regionID AND typeID = @typeID AND bid = @bid",
                    new Dictionary<string, object>()
                    {
                        {"@regionID", regionID},
                        {"@typeID", typeID},
                        {"@bid", TransactionType.Buy},
                        {"@currentSolarSystem", currentSolarSystem}
                    }
                ),
            };
        }

        private void BuildItemTypeList(ref Dictionary<int, List<int>> map, Dictionary<int, List<int>> marketToTypeID,
            Dictionary<int, List<int>> parentToMarket, int groupID)
        {
            // if the group exist do not do anything else
            if (map.ContainsKey(groupID) == false)
                map[groupID] = new List<int>();
            
            // look in our childs first
            if (parentToMarket.ContainsKey(groupID) == true)
            {
                foreach (int childrenGroupID in parentToMarket[groupID])
                {
                    this.BuildItemTypeList(ref map, marketToTypeID, parentToMarket, childrenGroupID);

                    // add to ourselves the list of items in the child group
                    map[groupID].AddRange(map[childrenGroupID]);
                }    
            }
            
            // add ourselves to the main list if theres typeIDs inside us
            if (marketToTypeID.ContainsKey(groupID) == true)
                map[groupID].AddRange(marketToTypeID[groupID]);
        }

        public PyDataType GetMarketGroups()
        {
            // this one is a messy boy, there is a util.FilterRowset which is just used here presumably
            // due to this being an exclusive case, better build it manually and call it a day
            Rowset result = Database.PrepareRowsetQuery("SELECT marketGroupID, parentGroupID, marketGroupName, description, graphicID, hasTypes, 0 AS types, 0 AS dataID FROM invMarketGroups ORDER BY parentGroupID");

            // build some dicts to know what points where
            Dictionary<int, List<int>> marketToTypeID = new Dictionary<int, List<int>>();
            // Dictionary<int, int> marketToParent = new Dictionary<int, int>();
            Dictionary<int, List<int>> parentToMarket = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> marketTypeIDsMap = new Dictionary<int, List<int>>();
        
            MySqlConnection connection = null;
            MySqlDataReader reader = null;
            
            reader = Database.Query(ref connection, "SELECT marketGroupID, parentGroupID FROM invMarketGroups");
            
            using (connection)
            using (reader)
            {
                while (reader.Read() == true)
                {
                    int child = reader.GetInt32(0);
                    int parent = reader.IsDBNull(1) == true ? -1 : reader.GetInt32(1);

                    if (parentToMarket.ContainsKey(parent) == false)
                        parentToMarket[parent] = new List<int>();
                    
                    parentToMarket[parent].Add(child);
                    // marketToParent[child] = parent;
                }
            }

            connection = null;
            
            reader = Database.Query(ref connection, "SELECT marketGroupID, typeID FROM invTypes WHERE marketGroupID IS NOT NULL ORDER BY marketGroupID");

            using (connection)
            using (reader)
            {
                while (reader.Read() == true)
                {
                    int marketGroupID = reader.GetInt32(0);
                    int typeID = reader.GetInt32(1);

                    if (marketToTypeID.ContainsKey(marketGroupID) == false)
                        marketToTypeID[marketGroupID] = new List<int>();

                    marketToTypeID[marketGroupID].Add(typeID);
                }
            }
            
            // maps for ids are already built, time to build the correct list of item types
            this.BuildItemTypeList(ref marketTypeIDsMap, marketToTypeID, parentToMarket, -1);
            
            PyDictionary finalResult = new PyDictionary();
            PyNone key = new PyNone();

            foreach (PyDataType entry in result.Rows)
            {
                PyList row = entry as PyList;
                PyInteger marketGroupID = row[0] as PyInteger;
                PyDataType parentGroupID = row[1];

                PyList types = new PyList();

                if (marketTypeIDsMap.ContainsKey(marketGroupID) == true)
                    foreach (int typeID in marketTypeIDsMap[marketGroupID])
                        types.Add(typeID);

                row[6] = types;

                PyDataType resultKey = parentGroupID ?? key;

                if (finalResult.ContainsKey(resultKey) == false)
                    finalResult[resultKey] = new PyList();
                
                (finalResult[resultKey] as PyList).Add(row);
            }

            return new PyObjectData("util.FilterRowset", new PyDictionary
                {
                    ["header"] = result.Header,
                    ["idName"] = "parentGroupID",
                    ["RowClass"] = new PyToken("util.Row"),
                    ["idName2"] = null,
                    ["items"] = finalResult
                }
            );
        }

        public CRowset GetOldPriceHistory(int regionID, int typeID)
        {
            return Database.PrepareCRowsetQuery(
                "SELECT historyDate, lowPrice, highPrice, avgPrice, volume, orders FROM mktHistoryOld WHERE regionID = @regionID AND typeID = @typeID",
                new Dictionary<string, object>()
                {
                    {"@regionID", regionID},
                    {"@typeID", typeID}
                }
            );
        }

        public CRowset GetNewPriceHistory(int regionID, int typeID)
        {
            return Database.PrepareCRowsetQuery(
                "SELECT transactionDateTime - (transactionDateTime % @dayLength) AS historyDate, MIN(price) AS lowPrice, MAX(price) AS highPrice, AVG(price) AS avgPrice, SUM(quantity) AS volume, COUNT(*) AS orders FROM mktTransactions WHERE regionID = @regionID AND typeID = @typeID AND transactionType = @transactionType GROUP BY historyDate",
                new Dictionary<string, object>()
                {
                    {"@dayLength", TimeSpan.TicksPerDay},
                    {"@regionID", regionID},
                    {"@typeID", typeID},
                    {"@transactionType", TransactionType.Buy}
                }
            );
        }
    }
}