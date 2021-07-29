using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Exceptions.marketProxy;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Market;
using Node.StaticData.Corporation;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public enum TransactionType : int
    {
        Sell = 0,
        Buy = 1,
        Either = 2,
    }
    
    public class MarketDB : DatabaseAccessor
    {
        private TypeManager TypeManager { get; }

        public Rowset GetNewTransactions(int entityID, int? clientID, TransactionType sellBuy, int? typeID, int quantity, int minPrice, int? accountKey)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"@entityID", entityID},
                {"@quantity", quantity},
                {"@price", minPrice}
            };
            
            string query =
                "SELECT transactionID, transactionDateTime, typeID, quantity, price, transactionType, characterID, IF(entityID = characterID, 0, 1) AS corpTransaction, clientID, stationID, accountKey AS keyID FROM mktTransactions WHERE entityID = @entityID AND quantity >= @quantity AND price >= @price";

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

            if (accountKey != null)
            {
                query += " AND accountKey = @accountKey";
                parameters["@accountKey"] = (int) accountKey;
            }
            
            return Database.PrepareRowsetQuery(query, parameters);
        }

        public Rowset GetCharOrders(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT orderID, typeID, charID, regionID, stationID, `range`, bid, price, volEntered, volRemaining, issued, minVolume, accountID, duration, isCorp, solarSystemID, escrow FROM mktOrders LEFT JOIN staStations USING (stationID) WHERE charID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public PyDictionary GetStationAsks(int stationID)
        {
            return Database.PrepareIntRowDictionary(
                "SELECT typeID, MAX(price) AS price, volRemaining, stationID FROM mktOrders WHERE stationID = @stationID GROUP BY typeID", 0,
                new Dictionary<string, object>()
                {
                    {"@stationID", stationID}
                }
            );
        }

        public PyDictionary GetSystemAsks(int solarSystemID)
        {
            return Database.PrepareIntRowDictionary(
                "SELECT typeID, MAX(price) AS price, volRemaining, stationID FROM mktOrders LEFT JOIN staStations USING (stationID) WHERE solarSystemID = @solarSystemID GROUP BY typeID", 0,
                new Dictionary<string, object>()
                {
                    {"@solarSystemID", solarSystemID}
                }
            );
        }

        public PyDictionary GetRegionBest(int regionID)
        {
            return Database.PrepareIntRowDictionary(
                "SELECT typeID, MAX(price) AS price, volRemaining, stationID FROM mktOrders LEFT JOIN staStations USING (stationID) WHERE regionID = @regionID GROUP BY typeID", 0,
                new Dictionary<string, object>()
                {
                    {"@regionID", regionID}
                }
            );
        }

        public PyList GetOrders(int regionID, int currentSolarSystem, int typeID)
        {
            return new PyList(2)
            {
                [0] = Database.PrepareCRowsetQuery(
                    "SELECT price, volRemaining, typeID, `range`, orderID, volEntered, minVolume, bid, issued, duration, stationID, regionID, solarSystemID, jumps FROM mktOrders LEFT JOIN staStations USING (stationID) LEFT JOIN mapPrecalculatedSolarSystemJumps ON mapPrecalculatedSolarSystemJumps.fromSolarSystemID = solarSystemID WHERE mapPrecalculatedSolarSystemJumps.toSolarSystemID = @currentSolarSystem AND regionID = @regionID AND typeID = @typeID AND bid = @bid",
                    new Dictionary<string, object>()
                    {
                        {"@regionID", regionID},
                        {"@typeID", typeID},
                        {"@bid", TransactionType.Sell},
                        {"@currentSolarSystem", currentSolarSystem}
                    }
                ),
                [1] = Database.PrepareCRowsetQuery(
                    "SELECT price, volRemaining, typeID, `range`, orderID, volEntered, minVolume, bid, issued, duration, stationID, regionID, solarSystemID, jumps FROM mktOrders LEFT JOIN staStations USING (stationID) LEFT JOIN mapPrecalculatedSolarSystemJumps ON mapPrecalculatedSolarSystemJumps.fromSolarSystemID = solarSystemID WHERE mapPrecalculatedSolarSystemJumps.toSolarSystemID = @currentSolarSystem AND regionID = @regionID AND typeID = @typeID AND bid = @bid",
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

        public PyObjectData GetMarketGroups()
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

            foreach (PyList row in result.Rows)
            {
                PyInteger marketGroupID = row[0] as PyInteger;
                PyDataType parentGroupID = row[1];

                PyList<PyInteger> types = new PyList<PyInteger>();

                if (marketTypeIDsMap.TryGetValue(marketGroupID, out List<int> typeIDsMap) == true)
                    foreach (int typeID in typeIDsMap)
                        types.Add(typeID);

                row[6] = types;

                PyDataType resultKey = parentGroupID ?? key;

                if (finalResult.TryGetValue(resultKey, out PyList values) == false)
                    finalResult[resultKey] = values = new PyList();

                values.Add(row);
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
                "SELECT transactionDateTime - (transactionDateTime % @dayLength) AS historyDate, MIN(price) AS lowPrice, MAX(price) AS highPrice, AVG(price) AS avgPrice, SUM(quantity) AS volume, COUNT(*) AS orders FROM mktTransactions LEFT JOIN staStations USING (stationID) WHERE regionID = @regionID AND typeID = @typeID AND transactionType = @transactionType GROUP BY historyDate",
                new Dictionary<string, object>()
                {
                    {"@dayLength", TimeSpan.TicksPerDay},
                    {"@regionID", regionID},
                    {"@typeID", typeID},
                    {"@transactionType", TransactionType.Buy}
                }
            );
        }

        public int CountCharsOrders(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT COUNT(*) FROM mktOrders WHERE charID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return 0;

                return reader.GetInt32(0);
            }
        }

        /// <summary>
        /// Special situation for Market
        ///
        /// Acquires the table lock for the orders table
        /// This allows to take exclusive control over it and perform any actions required
        /// </summary>
        /// <returns></returns>
        public MySqlConnection AcquireMarketLock()
        {
            MySqlConnection connection = null;
            Database.GetLock(ref connection, "market");

            return connection;
        }

        public MarketOrder[] FindMatchingOrders(MySqlConnection connection, double price, int typeID, int characterID, int solarSystemID, TransactionType type)
        {
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT orderID, typeID, charID, mktOrders.stationID AS locationID, price, mktOrders.accountID, volRemaining, minVolume, `range`, jumps, escrow, issued, chrInformation.corporationID, isCorp FROM mktOrders LEFT JOIN chrInformation ON charID = characterID LEFT JOIN staStations ON staStations.stationID = mktOrders.stationID LEFT JOIN mapPrecalculatedSolarSystemJumps ON staStations.solarSystemID = fromSolarSystemID AND toSolarsystemID = @solarSystemID WHERE bid = @transactionType AND price >= @price AND typeID = @typeID AND charID != @characterID AND `range` >= jumps ORDER BY price",
                new Dictionary<string, object>()
                {
                    {"@transactionType", type},
                    {"@price", price},
                    {"@typeID", typeID},
                    {"@solarSystemID", solarSystemID},
                    {"@characterID", characterID}
                }
            );

            using (reader)
            {
                List<MarketOrder> orders = new List<MarketOrder>();
                
                while (reader.Read() == true)
                {
                    // build the MarketOrder object
                    orders.Add(
                        new MarketOrder(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetInt32(2),
                            reader.GetInt32(3),
                            reader.GetDouble(4),
                            reader.GetInt32(5),
                            reader.GetInt32(6),
                            reader.GetInt32(7),
                            reader.GetInt32(8),
                            reader.GetInt32(9),
                            reader.IsDBNull(10) == false ? reader.GetDouble(10) : 0,
                            type,
                            reader.GetInt64(11),
                            reader.GetInt32(12),
                            reader.GetBoolean(13)
                        )
                    );
                }

                return orders.ToArray();
            }
        }

        public void UpdateOrderRemainingQuantity(MySqlConnection connection, int orderID, int newQuantityRemaining, double escrowCost)
        {
            Database.PrepareQuery(ref connection, "UPDATE mktOrders SET volRemaining = @quantity, escrow = escrow - @escrowCost WHERE orderID = @orderID",
                new Dictionary<string, object>()
                {
                    {"@orderID", orderID},
                    {"@quantity", newQuantityRemaining},
                    {"@escrowCost", escrowCost}
                }
            ).Close();
        }

        public void UpdatePrice(MySqlConnection connection, int orderID, double newPrice, double newEscrow)
        {
            Database.PrepareQuery(ref connection, "UPDATE mktOrders SET price = @price, escrow = @escrowCost WHERE orderID = @orderID",
                new Dictionary<string, object>()
                {
                    {"@orderID", orderID},
                    {"@price", newPrice},
                    {"@escrowCost", newEscrow}
                }
            ).Close();
        }

        public void RemoveOrder(MySqlConnection connection, int orderID)
        {
            Database.PrepareQuery(ref connection, "DELETE FROM mktOrders WHERE orderID = @orderID",
                new Dictionary<string, object>()
                {
                    {"@orderID", orderID}
                }
            ).Close();
        }

        public class ItemQuantityEntry
        {
            public int ItemID { get; set; }
            public int Quantity { get; set; }
            public int OriginalQuantity { get; set; }
            public int NodeID { get; set; }
            public double Damage { get; set; }
            public int LocationID { get; set; }
        }

        public Dictionary<int, ItemQuantityEntry> PrepareItemForOrder(MySqlConnection connection, int typeID, int stationID, int locationID2, int quantity, int ownerID1, int corporationID, long corporationRoles)
        {
            MySqlDataReader reader = null;

            if (CorporationRole.HangarCanTake1.Is(corporationRoles) == true ||
                CorporationRole.HangarCanTake2.Is(corporationRoles) == true ||
                CorporationRole.HangarCanTake3.Is(corporationRoles) == true ||
                CorporationRole.HangarCanTake4.Is(corporationRoles) == true ||
                CorporationRole.HangarCanTake5.Is(corporationRoles) == true ||
                CorporationRole.HangarCanTake6.Is(corporationRoles) == true ||
                CorporationRole.HangarCanTake7.Is(corporationRoles) == true)
            {
                reader = Database.PrepareQuery(ref connection,
                    "SELECT invItems.itemID, quantity, singleton, nodeID, IF(valueInt IS NULL, valueFloat, valueInt) AS damage, flag, ownerID, locationID FROM invItems LEFT JOIN invItemsAttributes ON invItemsAttributes.itemID = invItems.itemID AND invItemsAttributes.attributeID = @damageAttributeID WHERE typeID = @typeID AND (locationID = @locationID1 OR locationID = @locationID2 OR locationID = (SELECT officeID FROM crpOffices WHERE corporationID = @corporationID AND stationID = @locationID1)) AND (ownerID = @ownerID1 OR ownerID = @ownerID2)",
                    new Dictionary<string, object>()
                    {
                        {"@locationID1", stationID},
                        {"@locationID2", locationID2},
                        {"@ownerID1", ownerID1},
                        {"@ownerID2", corporationID},
                        {"@typeID", typeID},
                        {"@corporationID", corporationID},
                        {"@damageAttributeID", (int) Attributes.damage}
                    }
                );
            }
            else
            {
                reader = Database.PrepareQuery(ref connection,
                    "SELECT invItems.itemID, quantity, singleton, nodeID, IF(valueInt IS NULL, valueFloat, valueInt) AS damage, flag, ownerID, locationID FROM invItems LEFT JOIN invItemsAttributes ON invItemsAttributes.itemID = invItems.itemID AND invItemsAttributes.attributeID = @damageAttributeID WHERE typeID = @typeID AND (locationID = @locationID1 OR locationID = @locationID2) AND (ownerID = @ownerID1 OR ownerID = @ownerID2)",
                    new Dictionary<string, object>()
                    {
                        {"@locationID1", stationID},
                        {"@locationID2", locationID2},
                        {"@ownerID1", ownerID1},
                        {"@ownerID2", corporationID},
                        {"@typeID", typeID},
                        {"@damageAttributeID", (int) Attributes.damage}
                    }
                );
            }
            
            Dictionary<int, ItemQuantityEntry> itemIDToQuantityLeft = new Dictionary<int, ItemQuantityEntry>();
            int quantityLeft = quantity;
            
            using (reader)
            {
                while (reader.Read() == true)
                {
                    int itemID = reader.GetInt32(0);
                    int itemQuantity = reader.GetInt32(1);
                    int flag = reader.GetInt32(5);
                    int ownerID = reader.GetInt32(6);
                    int locationID = reader.GetInt32(7);

                    // check that the item is accessible to the character
                    // also ignore singletons too
                    if (
                        (CorporationRole.HangarCanTake1.Is(corporationRoles) == false && flag == (int) Flags.Hangar && ownerID == corporationID) || 
                        (CorporationRole.HangarCanTake2.Is(corporationRoles) == false && flag == (int) Flags.CorpSAG2 && ownerID == corporationID) || 
                        (CorporationRole.HangarCanTake3.Is(corporationRoles) == false && flag == (int) Flags.CorpSAG3 && ownerID == corporationID) || 
                        (CorporationRole.HangarCanTake4.Is(corporationRoles) == false && flag == (int) Flags.CorpSAG4 && ownerID == corporationID) || 
                        (CorporationRole.HangarCanTake5.Is(corporationRoles) == false && flag == (int) Flags.CorpSAG5 && ownerID == corporationID) || 
                        (CorporationRole.HangarCanTake6.Is(corporationRoles) == false && flag == (int) Flags.CorpSAG6 && ownerID == corporationID) || 
                        (CorporationRole.HangarCanTake7.Is(corporationRoles) == false && flag == (int) Flags.CorpSAG7 && ownerID == corporationID) ||
                        reader.GetBoolean(2) == true)
                        continue;
                    // TODO: CHECK HOW LIVE HANDLES TAKING THINGS FROM OFFICES

                    ItemQuantityEntry entry = new ItemQuantityEntry()
                    {
                        ItemID = itemID,
                        OriginalQuantity = itemQuantity,
                        Quantity = itemQuantity - Math.Min(itemQuantity, quantityLeft),
                        NodeID = reader.GetInt32(3),
                        Damage = reader.GetDoubleOrDefault(4),
                        LocationID = locationID
                    };

                    itemIDToQuantityLeft[itemID] = entry;

                    quantityLeft -= Math.Min(itemQuantity, quantityLeft);

                    if (quantityLeft == 0)
                        break;
                }
            }

            if (quantityLeft > 0)
            {
                // there's not enough items for this sale!!
                return null;
            }
            
            // preeliminary check, are items damaged?
            foreach ((int _, ItemQuantityEntry entry) in itemIDToQuantityLeft)
            {
                if (entry.Damage > 0)
                    throw new RepairBeforeSelling(this.TypeManager[typeID]);
            }
            
            // now iterate all the itemIDs, the ones that have a quantity of 0 must be moved to the correct container
            foreach ((int itemID, ItemQuantityEntry entry) in itemIDToQuantityLeft)
            {
                // if the item is loaded in any node do not update the item
                // that's the responsibility of the other node
                if (entry.NodeID != 0)
                    continue;
                
                // the item is just gone, remove it
                if (entry.Quantity == 0)
                {
                    Database.PrepareQuery(ref connection, "DELETE FROM invItems WHERE itemID = @itemID",
                        new Dictionary<string, object>()
                        {
                            {"@itemID", itemID}
                        }
                    ).Close();
                }
                else
                {
                    // reduce the item quantity available
                    Database.PrepareQuery(ref connection,
                        "UPDATE invItems SET quantity = @quantity WHERE itemID = @itemID",
                        new Dictionary<string, object>()
                        {
                            {"@itemID", itemID},
                            {"@quantity", entry.Quantity}
                        }
                    ).Close();
                }
            }

            // everything should be up to date, return the list of changes done so the market can notify the required nodes
            return itemIDToQuantityLeft;
        }

        public void CheckRepackagedItem(MySqlConnection connection, int itemID, out bool singleton)
        {
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT singleton FROM invItems WHERE itemID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@itemID", itemID}
                }
            );

            using (reader)
            {
                if (reader.Read() == false)
                {
                    // just in case do not allow the user to sell this item
                    singleton = true;
                    return;
                }

                singleton = reader.GetBoolean(0);
            }
        }

        public void PlaceSellOrder(MySqlConnection connection, int typeID, int ownerID, int stationID, int range, double price, int volEntered, int accountID, long duration, bool isCorp)
        {
            Database.PrepareQuery(ref connection,
                "INSERT INTO mktOrders(typeID, charID, stationID, `range`, bid, price, volEntered, volRemaining, issued, minVolume, accountID, duration, isCorp, escrow)VALUES(@typeID, @ownerID, @stationID, @range, @sell, @price, @volEntered, @volRemaining, @issued, @minVolume, @accountID, @duration, @isCorp, @escrow)",
                new Dictionary<string, object>()
                {
                    {"@typeID", typeID},
                    {"@ownerID", ownerID},
                    {"@stationID", stationID},
                    {"@range", range},
                    {"@sell", TransactionType.Sell},
                    {"@price", price},
                    {"@volEntered", volEntered},
                    {"@volRemaining", volEntered},
                    {"@issued", DateTime.UtcNow.ToFileTimeUtc()},
                    {"@minVolume", 0},
                    {"@accountID", accountID},
                    {"@duration", duration},
                    {"@isCorp", isCorp},
                    {"@escrow", 0}
                }
            ).Close();
        }

        public void PlaceBuyOrder(MySqlConnection connection, int typeID, int characterID, int stationID, int range, double price, int volEntered, int minVolume, int accountID, long duration, bool isCorp)
        {
            Database.PrepareQuery(ref connection,
                "INSERT INTO mktOrders(typeID, charID, stationID, `range`, bid, price, volEntered, volRemaining, issued, minVolume, accountID, duration, isCorp, escrow)VALUES(@typeID, @ownerID, @stationID, @range, @sell, @price, @volEntered, @volRemaining, @issued, @minVolume, @accountID, @duration, @isCorp, @escrow)",
                new Dictionary<string, object>()
                {
                    {"@typeID", typeID},
                    {"@ownerID", characterID},
                    {"@stationID", stationID},
                    {"@range", range},
                    {"@sell", TransactionType.Buy},
                    {"@price", price},
                    {"@volEntered", volEntered},
                    {"@volRemaining", volEntered},
                    {"@issued", DateTime.UtcNow.ToFileTimeUtc()},
                    {"@minVolume", minVolume},
                    {"@accountID", accountID},
                    {"@duration", duration},
                    {"@isCorp", isCorp},
                    {"@escrow", price * volEntered}
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
        public void ReleaseMarketLock(MySqlConnection connection)
        {
            Database.ReleaseLock(connection, "market");
        }

        public MarketOrder GetOrderById(MySqlConnection connection, int orderID)
        {
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT orderID, typeID, charID, mktOrders.stationID AS locationID, price, mktOrders.accountID, volRemaining, minVolume, `range`, escrow, bid, issued, corporationID, isCorp FROM mktOrders LEFT JOIN chrInformation ON charID = characterID WHERE orderID = @orderID",
                new Dictionary<string, object>()
                {
                    {"@orderID", orderID}
                }
            );

            using (reader)
            {
                if (reader.Read() == false)
                    return null;

                return new MarketOrder(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetInt32(2),
                    reader.GetInt32(3),
                    reader.GetDouble(4),
                    reader.GetInt32(5),
                    reader.GetInt32(6),
                    reader.GetInt32(7),
                    reader.GetInt32(8),
                    0,
                    reader.IsDBNull(9) == false ? reader.GetDouble(9) : 0,
                    reader.GetInt32(10) == ((int) TransactionType.Buy) ? TransactionType.Buy : TransactionType.Sell,
                    reader.GetInt64(11),
                    reader.GetInt32(12),
                    reader.GetBoolean(13)
                );
            }
        }

        public List<MarketOrder> GetExpiredOrders(MySqlConnection connection)
        {
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT orderID, typeID, charID, mktOrders.stationID AS locationID, price, mktOrders.accountID, volRemaining, minVolume, `range`, escrow, bid, issued, corporationID, isCorp FROM mktOrders LEFT JOIN chrInformation ON charID = characterID WHERE (issued + (duration * @ticksPerHour)) < @currentTime",
                new Dictionary<string, object>()
                {
                    {"@ticksPerHour", TimeSpan.TicksPerDay},
                    {"@currentTime", DateTime.UtcNow.ToFileTimeUtc()}
                }
            );

            using (reader)
            {
                List<MarketOrder> orders = new List<MarketOrder>();
                
                while (reader.Read() == true)
                {
                    orders.Add(new MarketOrder(
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.GetInt32(2),
                        reader.GetInt32(3),
                        reader.GetDouble(4),
                        reader.GetInt32(5),
                        reader.GetInt32(6),
                        reader.GetInt32(7),
                        reader.GetInt32(8),
                        0,
                        reader.IsDBNull(9) == false ? reader.GetDouble(9) : 0,
                        reader.GetInt32(10) == ((int) TransactionType.Buy) ? TransactionType.Buy : TransactionType.Sell,
                        reader.GetInt64(11),
                        reader.GetInt32(12),
                        reader.GetBoolean(13)
                    ));
                }

                return orders;
            }
        }

        public MarketDB(TypeManager typeManager, DatabaseConnection db) : base(db)
        {
            this.TypeManager = typeManager;
        }
    }
}