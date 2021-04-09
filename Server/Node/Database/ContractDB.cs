/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Exceptions.contractMgr;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Services.Contracts;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class ContractDB : DatabaseAccessor
    {
        private TypeManager TypeManager { get; }
        
        public ContractDB(TypeManager typeManager, DatabaseConnection db) : base(db)
        {
            this.TypeManager = typeManager;
        }

        public int GetOutstandingContractsCountForPlayer(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                "SELECT COUNT(*) AS contractCount FROM conContracts WHERE issuerID = @characterID and forCorp = @forCorp AND status = @outstandingStatus AND dateExpired > @currentTime",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@forCorp", 0},
                    {"@outstandingStatus", ContractStatus.Outstanding},
                    {"@currentTime", DateTime.UtcNow.ToFileTimeUtc()}
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
        
        public PyDataType NumRequiringAttention(int characterID, int corporationID)
        {
            return Database.PrepareKeyValQuery(
                "SELECT" +
                " (SELECT COUNT(*) FROM conContracts WHERE forCorp = @notForCorp AND ((issuerID = @characterID AND dateExpired < @currentTime) OR (acceptorID = @characterID AND dateCompleted <= @currentTime))) AS n," +
                " (SELECT COUNT(*) FROM conContracts WHERE forCorp = @forCorp AND ((issuerCorpID = @corporationID AND dateExpired < @currentTime) OR (acceptorCorpID = @corporationID AND dateCompleted <= @currentTime))) AS ncorp",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@forCorp", 1},
                    {"@notForCorp", 0},
                    {"@currentTime", DateTime.Now.ToFileTimeUtc()}
                }
            );
        }

        public PyDataType NumOutstandingContracts(int characterID, int corporationID)
        {
            return Database.PrepareKeyValQuery(
                "SELECT" +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @corporationID AND forCorp = @forCorp AND status = @outstandingStatus AND dateExpired > @currentTime) AS myCorpTotal," +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @characterID AND forCorp = @notForCorp AND status = @outstandingStatus AND dateExpired > @currentTime) AS myCharTotal," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID AND forCorp = @notForCorp AND status = @outstandingStatus AND dateExpired > @currentTime) AS nonCorpForMyCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID AND forCorp = @notForCorp AND status = @outstandingStatus AND dateExpired > @currentTime) AS nonCorpForMyChar",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@forCorp", 1},
                    {"@notForCorp", 0},
                    {"@outstandingStatus", (int) ContractStatus.Outstanding},
                    {"@currentTime", DateTime.UtcNow.ToFileTimeUtc()}
                }
            );
        }

        public PyDataType CollectMyPageInfo(int characterID, int corporationID)
        {
            // TODO: PROPERLY IMPLEMENT THIS
            return Database.PrepareKeyValQuery(
                "SELECT" +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @outstandingStatus AND issuerID = @characterID AND forCorp = @notForCorp AND dateExpired > @currentTime) AS numOutstandingContractsNonCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @outstandingStatus AND issuerCorpID = @corporationID AND forCorp = @forCorp AND dateExpired > @currentTime) AS numOutstandingContractsForCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @outstandingStatus AND issuerID = @characterID OR issuerCorpID = @corporationID) AS numOutstandingContracts," +
                " (SELECT COUNT(*) FROM conContracts WHERE forCorp = @notForCorp AND ((issuerID = @characterID AND dateExpired < @currentTime) OR (acceptorID = @characterID AND dateCompleted <= @currentTime))) AS numRequiresAttention," +
                " (SELECT COUNT(*) FROM conContracts WHERE forCorp = @forCorp AND ((issuerCorpID = @corporationID AND dateExpired < @currentTime) OR (acceptorCorpID = @corporationID AND dateCompleted <= @currentTime))) AS numRequiresAttentionCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID) AS numAssignedTo," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID) AS numAssignedToCorp," +
                " (SELECT COUNT(*) FROM conBids LEFT JOIN conContracts USING(contractID) WHERE conBids.issuerID = @characterID AND status = @outstandingStatus AND forCorp = @notForCorp) AS numBiddingOn," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @inProgressStatus AND issuerID = @characterID AND forCorp = @notForCorp) AS numInProgress," +
                " (SELECT COUNT(*) FROM conBids LEFT JOIN conContracts USING(contractID) WHERE conBids.issuerCorpID = @corporationID AND status = @outstandingStatus AND forCorp = @forCorp) AS numBiddingOnCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @inProgressStatus AND issuerCorpID = @corporationID AND forCorp = @forCorp) AS numInProgressCorp",
                new Dictionary<string, object>()
                {
                    {"@outstandingStatus", (int) ContractStatus.Outstanding},
                    {"@inProgressStatus", (int) ContractStatus.InProgress},
                    {"@notForCorp", 0},
                    {"@forCorp", 1},
                    {"@corporationID", corporationID},
                    {"@characterID", characterID},
                    {"@currentTime", DateTime.UtcNow.ToFileTimeUtc()}
                }
            );
        }

        public Rowset GetItemsInStationForPlayer(int characterID, int stationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT itemID, typeID, categoryID, groupID, singleton, quantity, flag, contraband FROM invItems LEFT JOIN invTypes USING (typeID) LEFT JOIN invGroups USING (groupID) WHERE ownerID = @characterID AND locationID = @stationID AND flag = @flagHangar",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@stationID", stationID},
                    {"@flagHangar", ItemFlags.Hangar}
                }
            );
        }

        public ulong CreateContract(MySqlConnection connection, int characterID, int corporationID, int? allianceID, ContractTypes type, int availability,
            int? assigneeID, int expireTime, int duration, int startStationID, int? endStationID, double price,
            double reward, double collateral, string title, string description, int issuerWalletID)
        {
            return Database.PrepareQueryLID(ref connection,
                "INSERT INTO conContracts(issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, endStationID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, issuerWalletKey, issuerAllianceID, acceptorWalletKey)VALUES(@issuerID, @issuerCorpID, @type, @availability, @assigneeID, @expiretime, @numDays, @startStationID, @endStationID, @price, @reward, @collateral, @title, @description, @forCorp, @status, @isAccepted, @acceptorID, @dateIssued, @dateExpired, @dateAccepted, @dateCompleted, @issuerWalletKey, @issuerAllianceID, @acceptorWalletKey)",
                new Dictionary<string, object>()
                {
                    {"@issuerID", characterID},
                    {"@issuerCorpID", corporationID},
                    {"@type", (int) type},
                    {"@availability", availability},
                    {"@assigneeID", assigneeID},
                    {"@expiretime", expireTime},
                    {"@numDays", duration},
                    {"@startStationID", startStationID},
                    {"@endStationID", endStationID},
                    {"@price", price},
                    {"@reward", reward},
                    {"@collateral", collateral},
                    {"@title", title},
                    {"@description", description},
                    {"@forCorp", 0},
                    {"@status", (int) ContractStatus.Outstanding},
                    {"@isAccepted", 0},
                    {"@acceptorID", null},
                    {"@dateIssued", DateTime.UtcNow.ToFileTimeUtc ()},
                    {"@dateExpired", DateTime.UtcNow.AddMinutes(expireTime).ToFileTimeUtc ()},
                    {"@dateAccepted", null},
                    {"@dateCompleted", null},
                    {"@issuerWalletKey", issuerWalletID},
                    {"@issuerAllianceID", allianceID},
                    {"@acceptorWalletKey", null}
                }
            );
        }

        public PyPackedRow GetContractInformation(int contractID, int characterID, int corporationID)
        {
            return Database.PreparePackedRowQuery(
                "SELECT contractID, issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, start.solarSystemID AS startSolarSystemID, start.regionID AS startRegionID, endStationID, end.solarSystemID AS endSolarSystemID, end.regionID AS endRegionID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, volume, crateID, issuerWalletKey, issuerAllianceID, acceptorWalletKey FROM conContracts LEFT JOIN staStations AS start ON start.stationID = startStationID LEFT JOIN staStations AS end ON end.stationID = endStationID WHERE ((availability = 1 AND (issuerID = @characterID OR issuerCorpID = @corporationID OR assigneeID = @characterID OR assigneeID = @corporationID OR acceptorID = @characterID OR acceptorID = @corporationID)) OR availability = 0) AND contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID},
                    {"@characterID", characterID},
                    {"@corporationID", corporationID}
                }
            );
        }
        
        public Rowset GetContractBids(int contractID, int characterID, int corporationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT bidID, contractID, conBids.issuerID, quantity, conBids.issuerCorpID, issuerStationID FROM conBids LEFT JOIN conContracts USING(contractID) WHERE ((availability = 1 AND (conContracts.issuerID = @characterID OR conContracts.issuerCorpID = @corporationID OR assigneeID = @characterID OR assigneeID = @corporationID OR acceptorID = @characterID OR acceptorID = @corporationID)) OR availability = 0) AND contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@contractID", contractID}
                }
            );
        }
        
        public Rowset GetContractItems(int contractID, int characterID, int corporationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT itemTypeID AS typeID, quantity, inCrate, itemID, materialLevel, productivityLevel, licensedProductionRunsRemaining AS bpRuns FROM conItems LEFT JOIN invBlueprints USING(itemID) LEFT JOIN conContracts USING(contractID) WHERE ((availability = 1 AND (conContracts.issuerID = @characterID OR conContracts.issuerCorpID = @corporationID OR assigneeID = @characterID OR assigneeID = @corporationID OR acceptorID = @characterID OR acceptorID = @corporationID)) OR availability = 0) AND contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@contractID", contractID}
                }
            );
        }

        public ContractStatus GetContractStatus(int contractID, int characterID, int corporationID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT status FROM conContracts WHERE ((availability = 1 AND (issuerID = @characterID OR issuerCorpID = @corporationID OR assigneeID = @characterID OR assigneeID = @corporationID OR acceptorID = @characterID OR acceptorID = @corporationID)) OR availability = 0) AND contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@contractID", contractID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return ContractStatus.InProgress;

                return (ContractStatus) reader.GetInt32(0);
            }
        }

        public class ItemQuantityEntry
        {
            public int ItemID { get; set; }
            public int TypeID { get; set; }
            public int Quantity { get; set; }
            public int NodeID { get; set; }
            public double Volume { get; set; }
        }
        
        public Dictionary<int, ItemQuantityEntry> PrepareItemsForContract(MySqlConnection connection, ulong contractID, PyList<PyList> itemList, Station station, int ownerID, int crateID, int shipID)
        {
            Dictionary<int, ItemQuantityEntry> items = new Dictionary<int, ItemQuantityEntry>();

            foreach (PyList itemEntryList in itemList)
            {
                PyList<PyInteger> itemEntry = itemEntryList.GetEnumerable<PyInteger>(); 
                PyInteger itemID = itemEntry[0];
                PyInteger quantity = itemEntry[1];

                if (itemID == shipID)
                    throw new ConCannotTradeCurrentShip();
                
                MySqlDataReader reader = Database.PrepareQuery(ref connection,
                    "SELECT quantity, nodeID, IF(dmg.valueFloat IS NULL, dmg.valueInt, dmg.valueFloat) AS damage, invItems.typeID, categoryID, singleton, contraband, IF(vol.attributeID IS NULL, IF(vold.valueFloat IS NULL, vold.valueInt, vold.valueFloat), IF(vol.valueFloat IS NULL, vol.valueInt, vol.valueFloat)) AS volume FROM invItems LEFT JOIN invTypes USING(typeID) LEFT JOIN invGroups USING(groupID) LEFT JOIN invItemsAttributes dmg ON invItems.itemID = dmg.itemID AND dmg.attributeID = @damage LEFT JOIN invItemsAttributes vol ON vol.itemID = invItems.itemID AND vol.attributeID = @volume LEFT JOIN dgmTypeAttributes vold ON vold.typeID = invItems.typeID AND vold.attributeID = @volume WHERE invItems.itemID = @itemID AND locationID = @locationID AND ownerID = @ownerID",
                    new Dictionary<string, object>()
                    {
                        {"@locationID", station.ID},
                        {"@ownerID", ownerID},
                        {"@itemID", itemID},
                        {"@damage", (int) AttributeEnum.damage},
                        {"@volume", (int) AttributeEnum.volume}
                    }
                );

                // TODO: CHECK FOR BLUEPRINT COPY TOO
                using (reader)
                {
                    if (reader.Read() == false)
                        throw new ConCannotTradeItemSanity();

                    int typeID = reader.GetInt32(3);
                    double volume = 0;

                    Type damageValue = reader.GetFieldType(2);

                    if (damageValue == typeof(long) && reader.GetInt64(2) > 0)
                        throw new ConCannotTradeDamagedItem(this.TypeManager[typeID].Name);
            
                    int itemQuantity = reader.GetInt32(0);

                    if (reader.GetInt32(4) == (int) ItemCategories.Ship && itemQuantity == 1 && reader.GetBoolean(5) == false)
                        throw new ConCannotTradeNonSingletonShip(this.TypeManager[typeID].Name, station.Name);

                    if (reader.GetBoolean(6) == true)
                        throw new ConCannotTradeContraband(this.TypeManager[typeID].Name);

                    // quantity MUST match for this operation to succeed
                    if (itemQuantity != quantity)
                        throw new ConCannotTradeItemSanity();

                    if (reader.IsDBNull(7) == false)
                        volume = reader.GetDouble(7);

                    ItemQuantityEntry entry = new ItemQuantityEntry()
                    {
                        ItemID = itemID,
                        TypeID = typeID,
                        Quantity = itemQuantity,
                        NodeID = reader.GetInt32(1),
                        Volume = volume * itemQuantity
                    };

                    items[itemID] = entry;
                }
            }
            
            // all the items pass the checks, move them in the database
            foreach ((int itemID, ItemQuantityEntry item) in items)
            {
                // create the records in the contract table
                Database.PrepareQuery(
                    ref connection,
                    "INSERT INTO conItems(contractID, itemTypeID, quantity, inCrate, itemID)VALUES(@contractID, @itemTypeID, @quantity, @inCrate, @itemID)",
                    new Dictionary<string, object>()
                    {
                        {"@contractID", contractID},
                        {"@itemTypeID", item.TypeID},
                        {"@quantity", item.Quantity},
                        {"@inCrate", 1},
                        {"@itemID", itemID}
                    }
                ).Close();
                
                // do not update the item in the database if the item belongs to any node
                if (item.NodeID != 0)
                    continue;
                
                Database.PrepareQuery(ref connection,
                    "UPDATE invItems SET locationID = @crateID WHERE itemID = @itemID",
                    new Dictionary<string, object>()
                    {
                        {"@itemID", itemID},
                        {"@crateID", crateID}
                    }
                ).Close();
            }

            return items;
        }

        public void UpdateContractCrateAndVolume(ref MySqlConnection connection, ulong contractID, int crateID, double volume)
        {
            Database.PrepareQuery(
                ref connection,
                "UPDATE conContracts SET crateID = @crateID, volume = @volume WHERE contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@crateID", crateID},
                    {"@volume", volume},
                    {"@contractID", contractID}
                }
            ).Close();
        }

        public PyList<PyPackedRow> GetItemsInContainer(int characterID, int containerID)
        {
            return Database.PreparePackedRowListQuery(
                "SELECT itemID, typeID, quantity FROM invItems WHERE locationID = @containerID",
                new Dictionary<string, object>()
                {
                    {"@containerID", containerID}
                }
            );
        }

        public void PrepareRequestedItems(MySqlConnection connection, ulong contractID, PyList<PyList> requestItemTypeList)
        {
            string query = "INSERT INTO conItems(contractID, itemTypeID, quantity, inCrate)VALUES";
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                {"@contractID", contractID},
                {"@inCrate", 0}
            };

            int entry = 0;
            
            foreach (PyList itemTypeList in requestItemTypeList)
            {
                PyList<PyInteger> itemEntry = itemTypeList.GetEnumerable<PyInteger>();
                PyInteger typeID = itemEntry[0];
                PyInteger quantity = itemEntry[1];

                query += $"(@contractID, @typeID{entry}, @quantity{entry}, @inCrate)";
                values[$"@typeID{entry}"] = typeID;
                values[$"@quantity{entry}"] = quantity;

                if (entry > 0)
                    query += ",";

                entry++;
            }
            
            // remove the last ','
            query.TrimEnd(',');

            Database.PrepareQuery(ref connection, query, values).Close();
        }
        
        public List<int> GetContractList(int? startContractID, int limit, int? itemTypeID, PyList<PyInteger> notIssuedByIDs, PyList<PyInteger> issuedByIDs, int? assigneeID, int? locationID, int? itemGroupID, int? itemCategoryID, int priceMax, int priceMin, int? type, string? description, int callerID, int callerCorpID, int? ownerID = null, int? status = null, bool includeExpired = false, bool expiredOnly = false)
        {
            string contractQuery = "SELECT contractID FROM conContracts LEFT JOIN conItems USING(contractID) LEFT JOIN invTypes ON conItems.itemTypeID = invTypes.typeID LEFT JOIN invGroups USING(groupID) LEFT JOIN staStations ON staStations.stationID = conContracts.startStationID WHERE (availability = 1 OR (availability = 0 AND issuerCorpID = @corporationID) OR assigneeID = @characterID)";
            Dictionary<string, object> values = new Dictionary<string, object>();
            
            if (startContractID > 0)
            {
                contractQuery += " AND contractID < @startContractID";
                values["@startContractID"] = startContractID;
            }

            if (itemTypeID is not null)
            {
                contractQuery += " AND itemTypeID = @itemTypeID";
                values["@itemTypeID"] = itemTypeID;
            }

            if (assigneeID is not null)
            {
                contractQuery += " AND assigneeID = @assigneeID";
                values["@assigneeID"] = assigneeID;
            }

            if (locationID is not null)
            {
                contractQuery += " AND (startStationID = @locationID OR solarSystemID = @locationID OR regionID = @locationID)";
                values["@locationID"] = locationID;
            }

            if (notIssuedByIDs is not null && notIssuedByIDs.Count > 0)
            {
                contractQuery += " AND (issuerID NOT IN (" + String.Join<PyInteger>(',', notIssuedByIDs) + ") AND forCorp = 0) AND (issuerCorpID NOT IN (" + String.Join<PyInteger>(',', notIssuedByIDs) + ") AND forCorp = 1)";
            }

            if (issuedByIDs is not null && issuedByIDs.Count > 0)
            {
                contractQuery += " AND ((issuerID IN (" + String.Join<PyInteger>(',', issuedByIDs) + ") AND forCorp = 0) OR (issuerCorpID IN (" + String.Join<PyInteger>(',', issuedByIDs) + ") AND forCorp = 1)";
            }

            if (itemCategoryID is not null)
            {
                contractQuery += " AND categoryID = @itemCategoryID";
                values["@itemCategoryID"] = itemCategoryID;
            }

            if (itemGroupID is not null)
            {
                contractQuery += " AND groupID = @itemGroupID";
                values["@itemGroupID"] = itemGroupID;
            }

            if (priceMax > 0)
            {
                contractQuery += " AND price < @priceMax";
                values["@priceMax"] = priceMax;
            }

            if (priceMin > 0)
            {
                contractQuery += " AND price > @priceMin";
                values["@priceMin"] = priceMin;
            }

            if (type is not null)
            {
                if (type == 10)
                {
                    contractQuery += " AND (type = @type1 OR type = @type2)";
                    values["@type1"] = (int) ContractTypes.ItemExchange;
                    values["@type2"] = (int) ContractTypes.Auction;
                }
                else
                {
                    contractQuery += " AND type = @type";
                    values["@type"] = type;
                }
            }

            if (description is not null)
            {
                contractQuery += " AND description LIKE @description";
                values["@description"] = $"%{description}%";
            }

            if (includeExpired == false)
            {
                contractQuery += " AND dateExpired > @currentTime";
                values["@currentTime"] = DateTime.UtcNow.ToFileTimeUtc();
            }

            if (expiredOnly == true)
            {
                contractQuery += " AND dateExpired <= @currentTime";
                values["@currentTime"] = DateTime.UtcNow.ToFileTimeUtc();
            }

            if (ownerID is not null)
            {
                contractQuery += " AND ((issuerID = @ownerID AND forCorp = 0) OR (issuerCorpID = @ownerID AND forCorp = 1))";
                values["@ownerID"] = ownerID;
            }

            if (status is not null)
            {
                contractQuery += " AND status = @status";
                values["@status"] = status;
            }

            // visibility checks are here too!
            values["@characterID"] = callerID;
            values["@corporationID"] = callerCorpID;

            if (limit > 0)
                contractQuery += $" LIMIT {limit}";

            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection, contractQuery, values);
            
            using (connection)
            using (reader)
            {
                List<int> result = new List<int>();
                
                while (reader.Read () == true)
                    result.Add(reader.GetInt32(0));

                // ensure that there's at least one id in the list so any fetch queries do not fail
                if (result.Count == 0)
                    result.Add(0);
                
                return result;
            }
        }

        public List<int> GetContractListByOwnerBids(int ownerID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT contractID FROM conBids WHERE issuerID = @ownerID OR issuerCorpID = @ownerID",
                new Dictionary<string, object>()
                {
                    {"@ownerID", ownerID}
                }
            );
            
            using(connection)
            using (reader)
            {
                List<int> result = new List<int>();
                
                while(reader.Read() == true)
                    result.Add(reader.GetInt32(0));

                // ensure that there's at least one id in the list so any fetch queries do not fail
                if (result.Count == 0)
                    result.Add(0);

                return result;

            }
        }

        public List<int> GetContractListByAcceptor(int acceptorID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT contractID FROM conContracts WHERE acceptorID = @acceptorID OR acceptorCorpID = @acceptorID",
                new Dictionary<string, object>()
                {
                    {"@acceptorID", acceptorID}
                }
            );
            
            using(connection)
            using (reader)
            {
                List<int> result = new List<int>();
                
                while(reader.Read() == true)
                    result.Add(reader.GetInt32(0));

                // ensure that there's at least one id in the list so any fetch queries do not fail
                if (result.Count == 0)
                    result.Add(0);

                return result;

            }
        }

        public PyDataType GetInformationForContractList(List<int> contractList)
        {
            return Database.PrepareCRowsetQuery(
                $"SELECT contractID, issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, start.solarSystemID AS startSolarSystemID, start.regionID AS startRegionID, endStationID, end.solarSystemID AS endSolarSystemID, end.regionID AS endRegionID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, volume, crateID, issuerWalletKey, issuerAllianceID, acceptorWalletKey FROM conContracts LEFT JOIN staStations AS start ON start.stationID = startStationID LEFT JOIN staStations AS end ON end.stationID = endStationID WHERE contractID IN ({String.Join(',', contractList)})"
            );
        }
        
        public PyDataType GetBidsForContractList(List<int> contractList)
        {
            return Database.PrepareIntPackedRowListDictionary(
                $"SELECT bidID, contractID, issuerID, quantity, issuerCorpID, issuerStationID FROM conBids WHERE contractID IN ({String.Join(',', contractList)}) ORDER BY contractID",
                1
            );
        }

        public PyDataType GetItemsForContractList(List<int> contractList)
        {
            return Database.PrepareIntPackedRowListDictionary(
                $"SELECT contractID, itemTypeID AS typeID, quantity, inCrate, materialLevel, productivityLevel, licensedProductionRunsRemaining AS bpRuns FROM conItems LEFT JOIN invBlueprints USING(itemID) WHERE contractID IN ({String.Join(',', contractList)}) ORDER BY contractID",
                0
            );
        }
    }
}