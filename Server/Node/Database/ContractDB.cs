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
                "SELECT COUNT(*) AS contractCount FROM conContracts WHERE issuerID = @characterID and forCorp = @forCorp AND status = @outstandingStatus",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@forCorp", 0},
                    {"@outstandingStatus", ContractStatus.Outstanding}
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
                " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @characterID AND forCorp = @notForCorp AND requiresAttentionByOwner = 1) + (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID AND forCorp = @notForCorp AND requiresAttentionByAssignee = 1) AS n," +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerCorpID = @corporationID AND forCorp = @forCorp AND requiresAttentionByOwner = 1) + (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID AND forCorp = @forCorp AND requiresAttentionByAssignee = 1) AS ncorp",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@forCorp", 1},
                    {"@notForCorp", 0}
                }
            );
        }

        public PyDataType NumOutstandingContracts(int characterID, int corporationID)
        {
            return Database.PrepareKeyValQuery(
                "SELECT" +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @corporationID AND forCorp = @forCorp AND status = @outstandingStatus) AS myCorpTotal," +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @characterID AND forCorp = @notForCorp AND status = @outstandingStatus) AS myCharTotal," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID AND forCorp = @notForCorp AND status = @outstandingStatus) AS nonCorpForMyCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID AND forCorp = @notForCorp AND status = @outstandingStatus) AS nonCorpForMyChar",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@forCorp", 1},
                    {"@notForCorp", 0},
                    {"@outstandingStatus", (int) ContractStatus.Outstanding}
                }
            );
        }

        public PyDataType CollectMyPageInfo(int characterID, int corporationID)
        {
            // TODO: PROPERLY IMPLEMENT THIS
            return Database.PrepareKeyValQuery(
                "SELECT" +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @outstandingStatus AND issuerID = @characterID AND forCorp = @notForCorp) AS numOutstandingContractsNonCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @outstandingStatus AND issuerCorpID = @corporationID AND forCorp = @forCorp) AS numOutstandingContractsForCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @outstandingStatus AND issuerID = @characterID OR issuerCorpID = @corporationID) AS numOutstandingContracts," +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @characterID AND forCorp = @notForCorp AND requiresAttentionByOwner = 1) + (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID AND forCorp = @notForCorp AND requiresAttentionByAssignee = 1) AS numRequiresAttention," +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerCorpID = @corporationID AND forCorp = @forCorp AND requiresAttentionByOwner = 1) + (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID AND forCorp = @forCorp AND requiresAttentionByAssignee = 1) AS numRequiresAttentionCorp," +
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
                    {"@characterID", characterID}
                }
            );
        }

        public CRowset GetContractsForOwner(int ownerID, int? contractType, int? contractStatus)
        {
            string contractQuery =
                "SELECT contractID, issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, start.solarSystemID AS startSolarSystemID, start.regionID AS startRegionID, endStationID, end.solarSystemID AS endSolarSystemID, end.regionID AS endRegionID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, volume, requiresAttentionByOwner, requiresAttentionByAssignee, crateID, issuerWalletKey, issuerAllianceID, acceptorWalletKey FROM conContracts LEFT JOIN staStations AS start ON start.stationID = startStationID LEFT JOIN staStations AS end ON end.stationID = endStationID WHERE ((issuerID = @ownerID AND forCorp = @notForCorp) OR (issuerCorpID = @ownerID AND forCorp = @forCorp))";

            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                {"@ownerID", ownerID},
                {"@notForCorp", 0},
                {"@forCorp", 1},
            };

            if (contractType != null)
            {
                contractQuery += " AND type = @contractType";
                values["@contractType"] = contractType;
            }

            if (contractStatus != null)
            {
                contractQuery += " AND status = @contractStatus";
                values["@contractStatus"] = contractStatus;
            }
            
            return Database.PrepareCRowsetQuery(contractQuery, values);
        }

        public PyDataType GetContractBidsForOwner(int ownerID, int? contractType, int? contractStatus)
        {
            string bidsQuery =
                "SELECT bidID, contractID, conBids.issuerID, quantity, conBids.issuerCorpID, issuerStationID FROM conBids LEFT JOIN conContracts USING (contractID) WHERE (conContracts.issuerID = @ownerID AND conContracts.forCorp = @notForCorp) OR (conContracts.issuerCorpID = @ownerID AND conContracts.forCorp = @forCorp)";
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                {"@ownerID", ownerID},
                {"@forCorp", 1},
                {"@notForCorp", 0}
            };
            
            if (contractType != null)
            {
                bidsQuery += " AND type = @contractType";
                values["@contractType"] = contractType;
            }

            if (contractStatus != null)
            {
                bidsQuery += " AND status = @contractStatus";
                values["@contractStatus"] = contractStatus;
            }
            
            return Database.PrepareIntRowDictionary(
                bidsQuery,
                1,
                values
            );
        }

        public PyDataType GetContractItemsForOwner(int ownerID, int? contractType, int? contractStatus)
        {
            string itemsQuery =
                "SELECT contractID, itemTypeID AS typeID, quantity, inCrate, materialLevel, productivityLevel, licensedProductionRunsRemaining AS bpRuns FROM conItems LEFT JOIN invBlueprints USING(itemID) LEFT JOIN conContracts USING (contractID) WHERE (issuerID = @ownerID AND forCorp = @notForCorp) OR (issuerCorpID = @ownerID AND forCorp = @forCorp) ORDER BY contractID";
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                {"@ownerID", ownerID},
                {"@notForCorp", 0},
                {"@forCorp", 1}
            };
            
            if (contractType != null)
            {
                itemsQuery += " AND type = @contractType";
                values["@contractType"] = contractType;
            }

            if (contractStatus != null)
            {
                itemsQuery += " AND status = @contractStatus";
                values["@contractStatus"] = contractStatus;
            }
            
            return Database.PrepareIntPackedRowListDictionary(
                itemsQuery,
                0,
                values
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
                "INSERT INTO conContracts(issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, endStationID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, requiresAttentionByOwner, requiresAttentionByAssignee, issuerWalletKey, issuerAllianceID, acceptorWalletKey)VALUES(@issuerID, @issuerCorpID, @type, @availability, @assigneeID, @expiretime, @numDays, @startStationID, @endStationID, @price, @reward, @collateral, @title, @description, @forCorp, @status, @isAccepted, @acceptorID, @dateIssued, @dateExpired, @dateAccepted, @dateCompleted, @requiresAttentionByOwner, @requiresAttentionByAssignee, @issuerWalletKey, @issuerAllianceID, @acceptorWalletKey)",
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
                    {"@requiresAttentionByOwner", 0},
                    {"@requiresAttentionByAssignee", 1},
                    {"@issuerWalletKey", issuerWalletID},
                    {"@issuerAllianceID", allianceID},
                    {"@acceptorWalletKey", null}
                }
            );
        }

        public PyPackedRow GetContractInformation(int contractID, int characterID, int corporationID)
        {
            return Database.PreparePackedRowQuery(
                "SELECT contractID, issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, start.solarSystemID AS startSolarSystemID, start.regionID AS startRegionID, endStationID, end.solarSystemID AS endSolarSystemID, end.regionID AS endRegionID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, volume, requiresAttentionByOwner, requiresAttentionByAssignee, crateID, issuerWalletKey, issuerAllianceID, acceptorWalletKey FROM conContracts LEFT JOIN staStations AS start ON start.stationID = startStationID LEFT JOIN staStations AS end ON end.stationID = endStationID WHERE ((availability = 1 AND (issuerID = @characterID OR issuerCorpID = @corporationID OR assigneeID = @characterID OR assigneeID = @corporationID OR acceptorID = @characterID OR acceptorID = @corporationID)) OR availability = 0) AND contractID = @contractID",
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
    }
}