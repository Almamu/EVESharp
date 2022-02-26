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
using System.Data;
using System.Diagnostics.Contracts;
using EVESharp.Common.Database;
using EVESharp.Node.Exceptions.contractMgr;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Services.Contracts;
using EVESharp.Node.StaticData.Inventory;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.Services.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Type = System.Type;

namespace EVESharp.Node.Database
{
    public class ContractDB : DatabaseAccessor
    {
        private TypeManager TypeManager { get; }

        public int GetOutstandingContractsCountForPlayer(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Select(
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
                " (SELECT COUNT(*) FROM conContracts WHERE forCorp = @forCorp AND ((issuerCorpID = @corporationID AND dateExpired < @currentTime) OR (acceptorID = @corporationID AND dateCompleted <= @currentTime))) AS ncorp",
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
                " (SELECT COUNT(*) FROM conContracts WHERE status = @outstandingStatus AND (issuerID = @characterID OR issuerCorpID = @corporationID) AND dateExpired > @currentTime) AS numOutstandingContracts," +
                " (SELECT COUNT(*) FROM conContracts WHERE forCorp = @notForCorp AND ((issuerID = @characterID AND dateExpired < @currentTime) OR (acceptorID = @characterID AND dateCompleted <= @currentTime))) AS numRequiresAttention," +
                " (SELECT COUNT(*) FROM conContracts WHERE (forCorp = @forCorp AND issuerCorpID = @corporationID AND dateExpired < @currentTime) OR (acceptorID = @corporationID AND dateCompleted <= @currentTime)) AS numRequiresAttentionCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID) AS numAssignedTo," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID) AS numAssignedToCorp," +
                " (SELECT COUNT(DISTINCT contractID) FROM conBids LEFT JOIN conContracts USING(contractID) WHERE bidderID = @characterID AND status = @outstandingStatus) AS numBiddingOn," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @inProgressStatus AND issuerID = @characterID AND forCorp = @notForCorp) AS numInProgress," +
                " (SELECT COUNT(DISTINCT contractID) FROM conBids LEFT JOIN conContracts USING(contractID) WHERE bidderID = @corporationID AND status = @outstandingStatus) AS numBiddingOnCorp," +
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
                    {"@flagHangar", Flags.Hangar}
                }
            );
        }

        public ulong CreateContract(MySqlConnection connection, int characterID, int corporationID, int? allianceID, ContractTypes type, int availability,
            int assigneeID, int expireTime, int duration, int startStationID, int? endStationID, double price,
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
                    {"@acceptorID", 0},
                    {"@dateIssued", DateTime.UtcNow.ToFileTimeUtc ()},
                    {"@dateExpired", DateTime.UtcNow.AddMinutes(expireTime).ToFileTimeUtc ()},
                    {"@dateAccepted", 0},
                    {"@dateCompleted", 0},
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
                "SELECT bidID, bidderID, amount FROM conBids WHERE contractID = @contractID ORDER BY amount DESC",
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
                "SELECT itemTypeID AS typeID, quantity, inCrate, itemID, materialLevel, productivityLevel, licensedProductionRunsRemaining AS bpRuns FROM conItems LEFT JOIN invBlueprints USING(itemID) WHERE contractID = @contractID",
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
            MySqlDataReader reader = Database.Select(ref connection,
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
            public int ItemID { get; init; }
            public int TypeID { get; init; }
            public int Quantity { get; set; }
            public int OldQuantity { get; init; }
            public int NodeID { get; init; }
            public double Volume { get; init; }
            public bool Singleton { get; init; }
            public bool Contraband { get; init; }
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
                
                MySqlDataReader reader = Database.Select(ref connection,
                    "SELECT quantity, nodeID, IF(dmg.valueFloat IS NULL, dmg.valueInt, dmg.valueFloat) AS damage, invItems.typeID, categoryID, singleton, contraband, IF(vol.attributeID IS NULL, IF(vold.valueFloat IS NULL, vold.valueInt, vold.valueFloat), IF(vol.valueFloat IS NULL, vol.valueInt, vol.valueFloat)) AS volume FROM invItems LEFT JOIN invTypes USING(typeID) LEFT JOIN invGroups USING(groupID) LEFT JOIN invItemsAttributes dmg ON invItems.itemID = dmg.itemID AND dmg.attributeID = @damage LEFT JOIN invItemsAttributes vol ON vol.itemID = invItems.itemID AND vol.attributeID = @volume LEFT JOIN dgmTypeAttributes vold ON vold.typeID = invItems.typeID AND vold.attributeID = @volume WHERE invItems.itemID = @itemID AND locationID = @locationID AND ownerID = @ownerID",
                    new Dictionary<string, object>()
                    {
                        {"@locationID", station.ID},
                        {"@ownerID", ownerID},
                        {"@itemID", itemID},
                        {"@damage", (int) Attributes.damage},
                        {"@volume", (int) Attributes.volume}
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

                    if (damageValue == typeof(long) && reader.IsDBNull(2) == false && reader.GetInt64(2) > 0)
                        throw new ConCannotTradeDamagedItem(this.TypeManager[typeID]);
                    if (damageValue == typeof(double) && reader.IsDBNull(2) == false && reader.GetDouble(2) > 0)
                        throw new ConCannotTradeDamagedItem(this.TypeManager[typeID]);
            
                    int itemQuantity = reader.GetInt32(0);

                    if (reader.GetInt32(4) == (int) Categories.Ship && itemQuantity == 1 && reader.GetBoolean(5) == false)
                        throw new ConCannotTradeNonSingletonShip(this.TypeManager[typeID], station.ID);

                    if (reader.GetBoolean(6) == true)
                        throw new ConCannotTradeContraband(this.TypeManager[typeID]);

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
                Database.Query(
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
                );
                
                // do not update the item in the database if the item belongs to any node
                if (item.NodeID != 0)
                    continue;
                
                Database.Query(ref connection,
                    "UPDATE invItems SET locationID = @crateID WHERE itemID = @itemID",
                    new Dictionary<string, object>()
                    {
                        {"@itemID", itemID},
                        {"@crateID", crateID}
                    }
                );
            }

            return items;
        }

        public void UpdateContractCrateAndVolume(ref MySqlConnection connection, ulong contractID, int crateID, double volume)
        {
            Database.Query(
                ref connection,
                "UPDATE conContracts SET crateID = @crateID, volume = @volume WHERE contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@crateID", crateID},
                    {"@volume", volume},
                    {"@contractID", contractID}
                }
            );
        }

        public void UpdateContractStatus(ref MySqlConnection connection, int contractID, ContractStatus newStatus)
        {
            Database.Query(
                ref connection,
                "UPDATE conContracts SET status = @status WHERE contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID},
                    {"@status", (int) newStatus}
                }
            );
        }

        public void UpdateAcceptorID(ref MySqlConnection connection, int contractID, int newAcceptorID)
        {
            Database.Query(
                ref connection,
                "UPDATE conContracts SET acceptorID = @acceptorID WHERE contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID},
                    {"@acceptorID", newAcceptorID}
                }
            );
        }

        public void UpdateAcceptedDate(ref MySqlConnection connection, int contractID)
        {
            Database.Query(
                ref connection,
                "UPDATE conContracts SET dateAccepted = @currentTime WHERE contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID},
                    {"@currentTime", DateTime.UtcNow.ToFileTimeUtc()}
                }
            );
        }

        public void UpdateCompletedDate(ref MySqlConnection connection, int contractID)
        {
            Database.Query(
                ref connection,
                "UPDATE conContracts SET dateCompleted = @currentTime WHERE contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID},
                    {"@currentTime", DateTime.UtcNow.ToFileTimeUtc()}
                }
            );
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

            Database.Query(ref connection, query, values);
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
                contractQuery += " AND ((issuerID NOT IN (" + String.Join<PyInteger>(',', notIssuedByIDs) + ") AND forCorp = 0) AND (issuerCorpID NOT IN (" + String.Join<PyInteger>(',', notIssuedByIDs) + ") AND forCorp = 1))";
            }

            if (issuedByIDs is not null && issuedByIDs.Count > 0)
            {
                contractQuery += " AND ((issuerID IN (" + String.Join<PyInteger>(',', issuedByIDs) + ") AND forCorp = 0) OR (issuerCorpID IN (" + String.Join<PyInteger>(',', issuedByIDs) + ") AND forCorp = 1))";
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
            MySqlDataReader reader = Database.Select(ref connection, contractQuery, values);
            
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
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT contractID FROM conBids WHERE bidderID = @ownerID",
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
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT contractID FROM conContracts WHERE acceptorID = @acceptorID AND status = @status",
                new Dictionary<string, object>()
                {
                    {"@acceptorID", acceptorID},
                    {"@status", (int) ContractStatus.InProgress}
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
                $"SELECT contractID, amount, bidderID FROM conBids WHERE contractID IN ({String.Join(',', contractList)}) ORDER BY contractID, amount DESC",
                0
            );
        }

        public PyDataType GetItemsForContractList(List<int> contractList)
        {
            return Database.PrepareIntPackedRowListDictionary(
                $"SELECT contractID, itemTypeID AS typeID, quantity, inCrate, materialLevel, productivityLevel, licensedProductionRunsRemaining AS bpRuns FROM conItems LEFT JOIN invBlueprints USING(itemID) WHERE contractID IN ({String.Join(',', contractList)}) ORDER BY contractID",
                0
            );
        }

        public ulong PlaceBid(MySqlConnection connection, int contractID, int amount, int bidderID, bool forCorp)
        {
            return Database.PrepareQueryLID(ref connection,
                "INSERT INTO conBids(contractID, bidderID, forCorp, amount)VALUES(@contractID, @bidderID, @forCorp, @amount)",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID},
                    {"@bidderID", bidderID},
                    {"@forCorp", forCorp},
                    {"@amount", amount},
                }
            );
        }

        public void GetOutbids(MySqlConnection connection, int contractID, int amount, out List<int> characterIDs, out List<int> corporationIDs)
        {
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT bidderID, forCorp FROM conBids WHERE contractID = @contractID GROUP BY bidderID",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID},
                    {"@amount", amount}
                }
            );
            
            using (reader)
            {
                characterIDs = new List<int>();
                corporationIDs = new List<int>();

                while (reader.Read() == true)
                {
                    if (reader.GetBoolean(1) == true)
                    {
                        corporationIDs.Add(reader.GetInt32(0));
                    }
                    else
                    {
                        characterIDs.Add(reader.GetInt32(0));
                    }
                }
            }
        }

        public void GetMaximumBid(MySqlConnection connection, int contractID, out int bidderID, out int amount)
        {
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT amount, bidderID FROM conBids WHERE contractID = @contractID ORDER BY amount DESC LIMIT 1",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID}
                }
            );
            
            using (reader)
            {
                if (reader.Read() == false)
                {
                    bidderID = 0;
                    amount = 0;
                }
                else
                {
                    amount = reader.GetInt32(0);
                    bidderID = reader.GetInt32(1);
                }
            }
        }

        public class Contract
        {
            public int ID { get; init; }
            public double? Price { get; init; }
            public int Collateral { get; init; }
            public long ExpireTime { get; init; }
            public int CrateID { get; init; }
            public int StationID { get; init; }
            public ContractStatus Status { get; init; }
            public ContractTypes Type { get; init; }
            public int IssuerID { get; init; }
            public int IssuerCorpID { get; init; }
            public bool ForCorp { get; init; }
            public double? Reward { get; init; }
            
        }

        public Contract GetContract(MySqlConnection connection, int contractID)
        {
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT price, collateral, status, type, dateExpired, crateID, startStationID, issuerID, issuerCorpID, forCorp, reward FROM conContracts WHERE contractID = @contractID",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID}
                }
            );
            
            using (reader)
            {
                if (reader.Read() == false)
                    return null;

                return new Contract()
                {
                    ID = contractID,
                    Price = reader.GetDoubleOrNull(0),
                    Collateral = reader.GetInt32(1),
                    Status = (ContractStatus) reader.GetInt32(2),
                    Type = (ContractTypes) reader.GetInt32(3),
                    ExpireTime = reader.GetInt64(4),
                    CrateID = reader.GetInt32(5),
                    StationID = reader.GetInt32(6),
                    IssuerID = reader.GetInt32(7),
                    IssuerCorpID = reader.GetInt32(8),
                    ForCorp = reader.GetBoolean(9),
                    Reward = reader.GetDoubleOrNull(10)
                };
            }
        }

        public List<int> FetchLoginCharacterContractBids(int bidderID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT contractID, MAX(amount), (SELECT MAX(amount) FROM conBids b WHERE b.contractID = contractID) AS maximum FROM conBids LEFT JOIN conContracts USING(contractID) WHERE bidderID = @bidderID AND status = @outstandingStatus AND dateExpired < @currentTime GROUP BY contractID",
                new Dictionary<string, object>()
                {
                    {"@bidderID", bidderID},
                    {"@outstandingStatus", ContractStatus.Outstanding},
                    {"@currentTime", DateTime.UtcNow.ToFileTimeUtc()}
                }
            );
            
            using(connection)
            using (reader)
            {
                List<int> result = new List<int>();

                while (reader.Read() == true)
                {
                    if (reader.GetInt32(1) != reader.GetInt32(2))
                        result.Add(reader.GetInt32(0));
                }

                return result;
            }
        }

        public List<int> FetchLoginCharacterContractAssigned(int assigneeID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT contractID FROM conContracts WHERE assigneeID = @assigneeID AND status = @outstandingStatus AND dateExpired < @currentTime",
                new Dictionary<string, object>()
                {
                    {"@assigneeID", assigneeID},
                    {"@outstandingStatus", ContractStatus.Outstanding},
                    {"@currentTime", DateTime.UtcNow.ToFileTimeUtc()}
                }
            );
            
            using(connection)
            using (reader)
            {
                List<int> result = new List<int>();

                while (reader.Read() == true)
                {
                    result.Add(reader.GetInt32(0));
                }

                return result;
            }
        }

        public Dictionary<int, int> GetRequiredItemTypeIDs(MySqlConnection connection, int contractID)
        {
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT itemTypeID, SUM(quantity) AS quantity FROM conItems WHERE contractID = @contractID AND inCrate = 0 GROUP BY itemTypeID",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID}
                }
            );

            using (reader)
            {
                Dictionary<int, int> result = new Dictionary<int, int>();

                while (reader.Read() == true)
                    result[reader.GetInt32(0)] = reader.GetInt32(1);

                return result;
            }
        }

        public List<ItemQuantityEntry> GetOfferedItems(MySqlConnection connection, int contractID)
        {
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT itemID, itemTypeID, conItems.quantity, nodeID FROM conItems LEFT JOIN invItems USING(itemID) WHERE contractID = @contractID AND inCrate = 1",
                new Dictionary<string, object>()
                {
                    {"@contractID", contractID}
                }
            );

            using (reader)
            {
                List<ItemQuantityEntry> result = new List<ItemQuantityEntry>();

                while (reader.Read() == true)
                    result.Add(new ItemQuantityEntry()
                        {
                            ItemID = reader.GetInt32(0),
                            TypeID = reader.GetInt32(1),
                            Quantity = reader.GetInt32(2)
                        }
                    );

                return result;
            }
        }

        public void ExtractCrate(MySqlConnection connection, int crateID, int newLocationID, int newOwnerID)
        {
            // take all the items out
            Database.Query(ref connection,
                "UPDATE invItems SET locationID = @newLocationID, ownerID = @newOwnerID WHERE locationID = @crateID",
                new Dictionary<string, object>()
                {
                    {"@newLocationID", newLocationID},
                    {"@newOwnerID", newOwnerID},
                    {"@crateID", crateID}
                }
            );
            // remove the crate
            Database.Query(ref connection,
                "DELETE FROM invItems WHERE itemID = @crateID",
                new Dictionary<string, object>()
                {
                    {"@crateID", crateID}
                }
            );
        }

        public List<ItemQuantityEntry> CheckRequiredItemsAtStation(MySqlConnection connection, Station station, int ownerID, int newOwnerID, Flags flag, Dictionary<int, int> requiredItemTypes)
        {
            MySqlDataReader reader = Database.Select(ref connection,
                $"SELECT invItems.itemID, quantity, typeID, nodeID, IF(dmg.valueFloat IS NULL, dmg.valueInt, dmg.valueFloat) AS damage, categoryID, singleton, contraband FROM invItems LEFT JOIN invTypes USING(typeID) LEFT JOIN invGroups USING(groupID) LEFT JOIN invItemsAttributes dmg ON invItems.itemID = dmg.itemID AND dmg.attributeID = @damage WHERE typeID IN ({string.Join(',', requiredItemTypes.Keys)}) AND locationID = @locationID AND ownerID = @ownerID AND flag = @flag",
                new Dictionary<string, object>()
                {
                    {"@locationID", station.ID},
                    {"@ownerID", ownerID},
                    {"@damage", (int) Attributes.damage},
                    {"@flag", (int) flag}
                }
            );

            List<ItemQuantityEntry> itemsAtStation = new List<ItemQuantityEntry>();
            
            using (reader)
            {
                while (reader.Read() == true)
                {
                    int typeID = reader.GetInt32(2);

                    Type damageValue = reader.GetFieldType(4);

                    if (damageValue == typeof(long) && reader.IsDBNull(4) == false && reader.GetInt64(4) > 0)
                        throw new ConCannotTradeDamagedItem(this.TypeManager[typeID]);
                    if (damageValue == typeof(double) && reader.IsDBNull(4) == false && reader.GetDouble(4) > 0)
                        throw new ConCannotTradeDamagedItem(this.TypeManager[typeID]);
                    if (reader.GetBoolean(6) == false && reader.GetInt32(6) == (int) Categories.Ship)
                        throw new ConCannotTradeNonSingletonShip(this.TypeManager[typeID], station.ID);
                    if (reader.GetBoolean(7) == true)
                        throw new ConCannotTradeContraband(this.TypeManager[typeID]);
                    
                    itemsAtStation.Add(
                        new ItemQuantityEntry()
                        {
                            ItemID = reader.GetInt32(0),
                            Quantity = reader.GetInt32(1),
                            OldQuantity = reader.GetInt32(1),
                            TypeID = typeID,
                            NodeID = reader.GetInt32(3),
                            Singleton = reader.GetBoolean(6),
                            Contraband = reader.GetBoolean(7),
                        }
                    );
                }
            }

            List<ItemQuantityEntry> modifiedItems = new List<ItemQuantityEntry>();
            
            // now that the list has been built check that the requirements are met
            foreach ((int itemTypeID, int requiredQuantity) in requiredItemTypes)
            {
                int quantity = requiredQuantity;
                // find all the items that match the criteria
                List<ItemQuantityEntry> matchingItems = itemsAtStation.FindAll(x => x.TypeID == itemTypeID);
                // now iterate them and ensure there's enough quantity
                foreach (ItemQuantityEntry item in matchingItems)
                {
                    if (item.Quantity > quantity)
                    {
                        item.Quantity -= quantity;
                        quantity = 0;
                    }
                    else
                    {
                        quantity -= item.Quantity;
                        item.Quantity = 0;
                    }
                    
                    // keep track of what items where modified
                    modifiedItems.Add(item);

                    if (quantity == 0)
                        break;
                }

                // if the quantity required is not exhausted there was an error
                if (quantity > 0)
                    throw new ConReturnItemsMissingNonSingleton(this.TypeManager[itemTypeID], station.ID);
            }
            
            // iterate the modified items and update database if required
            foreach (ItemQuantityEntry entry in modifiedItems)
            {
                // if the whole item changed it can be moved
                if (entry.Quantity == 0)
                    Database.Query(ref connection, "UPDATE invItems SET locationID = @locationID, ownerID = @newOwnerID WHERE itemID = @itemID",
                        new Dictionary<string, object>()
                        {
                            {"@itemID", entry.ItemID},
                            {"@newOwnerID", newOwnerID},
                            {"@locationID", station.ID}
                        }
                    );
                else
                    Database.Query(ref connection,
                        "UPDATE invItems SET quantity = @quantity WHERE itemID = @itemID",
                        new Dictionary<string, object>()
                        {
                            {"@itemID", entry.ItemID},
                            {"@quantity", entry.Quantity}
                        }
                    );
            }

            return modifiedItems;
        }

        public List<ItemQuantityEntry> GetCrateItems(MySqlConnection connection, int crateID)
        {
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT itemID, nodeID FROM conItems LEFT JOIN conContracts USING(contractID) LEFT JOIN invItems USING(itemID) WHERE crateID = @createID AND inCrate = 1",
                new Dictionary<string, object>()
                {
                    {"@crateID", crateID}
                }
            );

            using (reader)
            {
                List<ItemQuantityEntry> result = new List<ItemQuantityEntry>();

                while (reader.Read() == true)
                {
                    result.Add(new ItemQuantityEntry()
                        {
                            ItemID = reader.GetInt32(0),
                            NodeID = reader.GetInt32(1)
                        }
                    );
                }

                return result;
            }
        }

        public ContractDB(TypeManager typeManager, DatabaseConnection db) : base(db)
        {
            this.TypeManager = typeManager;
        }
    }
}