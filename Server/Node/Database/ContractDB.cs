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

        public PyDataType GetContractsForOwner(int characterID, int corporationID)
        {
            return Database.PrepareCRowsetQuery(
                "SELECT contractID, issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, start.solarSystemID AS startSolarSystemID, start.regionID AS startRegionID, endStationID, end.solarSystemID AS endSolarSystemID, end.regionID AS endRegionID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, volume, requiresAttentionByOwner, requiresAttentionByAssignee, crateID, issuerWalletKey, issuerAllianceID, acceptorWalletKey FROM conContracts LEFT JOIN staStations AS start ON start.stationID = startStationID LEFT JOIN staStations AS end ON end.stationID = endStationID WHERE (issuerID = @characterID AND forCorp = @notForCorp) OR (issuerCorpID = @corporationID AND forCorp = @forCorp)",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@notForCorp", 0},
                    {"@forCorp", 1}
                }
            );
        }

        public PyDataType GetContractBidsForOwner(int characterID, int corporationID)
        {
            return Database.PrepareIntRowDictionary(
                "SELECT bidID, contractID, conBids.issuerID, quantity, conBids.issuerCorpID, issuerStationID FROM conBids WHERE issuerID = @characterID OR issuerCorpID = @corporationID",
                1,
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID}
                }
            );
        }

        public PyDataType GetContractItemsForOwner(int characterID, int corporationID)
        {
            // TODO: INCLUDE BLUEPRINT INFORMATION!
            return Database.PrepareIntRowDictionary(
                "SELECT contractID, itemTypeID, quantity, inCrate FROM conItems LEFT JOIN conContracts USING (contractID) WHERE (issuerID = @characterID AND forCorp = @notForCorp) OR (issuerCorpID = @corporationID AND forCorp = @forCorp)",
                0,
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@notForCorp", 0},
                    {"@forCorp", 1}
                }
            );
        }

        public PyDataType GetItemsInStationForPlayer(int characterID, int stationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT itemID, typeID, categoryID, groupID, singleton, quantity, flag, contraband FROM invItems LEFT JOIN invTypes USING (typeID) LEFT JOIN invGroups USING (groupID) WHERE ownerID = @characterID AND locationID = @stationID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@stationID", stationID}
                }
            );
        }

        public ulong CreateContract(int characterID, int corporationID, int? allianceID, ContractTypes type, int availability,
            int? assigneeID, int expireTime, int duration, int startStationID, int endStationID, double price,
            double reward, double collateral, string title, string description, double volume, int crateID,
            int issuerWalletID)
        {
            return Database.PrepareQueryLID(
                "INSERT INTO conContracts(issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, endStationID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, volume, requiresAttentionByOwner, requiresAttentionByAssignee, crateID, issuerWalletKey, issuerAllianceID, acceptorWalletKey)VALUES(@issuerID, @issuerCorpID, @type, @availability, @assigneeID, @expiretime, @numDays, @startStationID, @endStationID, @price, @reward, @collateral, @title, @description, @forCorp, @status, @isAccepted, @acceptorID, @dateIssued, @dateExpired, @dateAccepted, @dateCompleted, @volume, @requiresAttentionByOwner, @requiresAttentionByAssignee, @crateID, @issuerWalletKey, @issuerAllianceID, @acceptorWalletKey)",
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
                    {"@dateExpired", null},
                    {"@dateAccepted", null},
                    {"@dateCompleted", null},
                    {"@volume", volume},
                    {"@requiresAttentionByOwner", 0},
                    {"@requiresAttentionByAssignee", 0},
                    {"@crateID", crateID},
                    {"@issuerWalletKey", issuerWalletID},
                    {"@issuerAllianceID", allianceID},
                    {"@acceptorWalletKey", null}
                }
            );
        }

        public PyDataType GetContractInformation(int contractID, int characterID, int corporationID)
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
        
        public PyDataType GetContractBids(int contractID, int characterID, int corporationID)
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
        
        public PyDataType GetContractItems(int contractID, int characterID, int corporationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT itemTypeID, quantity, inCrate, itemID FROM conItems  LEFT JOIN conContracts USING(contractID) WHERE ((availability = 1 AND (conContracts.issuerID = @characterID OR conContracts.issuerCorpID = @corporationID OR assigneeID = @characterID OR assigneeID = @corporationID OR acceptorID = @characterID OR acceptorID = @corporationID)) OR availability = 0) AND contractID = @contractID",
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
            public int NodeID { get; set; }
        }
        
        public Dictionary<int, ItemQuantityEntry> PrepareItemsForOrder(MySqlConnection connection, PyList<PyList<PyInteger>> itemList, Station station, int ownerID, int crateID, int shipID)
        {
            Dictionary<int, ItemQuantityEntry> items = new Dictionary<int, ItemQuantityEntry>();

            foreach (PyList<PyInteger> itemEntry in itemList)
            {
                PyInteger itemID = itemEntry[0];
                PyInteger quantity = itemEntry[1];

                if (itemID == shipID)
                    throw new ConCannotTradeCurrentShip();
                
                MySqlDataReader reader = Database.PrepareQuery(ref connection,
                    "SELECT quantity, nodeID, IF(valueFloat IS NULL, valueInt, valueFloat) AS value, typeID, categoryID, singleton, contraband FROM invItems LEFT JOIN invTypes USING(typeID) LEFT JOIN invGroups USING(groupID) LEFT JOIN invItemsAttributes USING(itemID) WHERE itemID = @itemID AND locationID = @locationID AND ownerID = @ownerID AND attributeID = @damage",
                    new Dictionary<string, object>()
                    {
                        {"@locationID", station.ID},
                        {"@ownerID", ownerID},
                        {"@itemID", itemID},
                        {"@damage", (int) AttributeEnum.damage}
                    }
                );

                // TODO: CHECK FOR BLUEPRINT COPY TOO
                using (reader)
                {
                    if (reader.Read() == false)
                        throw new ConCannotTradeItemSanity();

                    Type damageValue = reader.GetFieldType(2);

                    if (damageValue == typeof(long) && reader.GetInt64(2) > 0)
                        throw new ConCannotTradeDamagedItem(this.TypeManager[reader.GetInt32(3)].Name);
            
                    int itemQuantity = reader.GetInt32(0);

                    if (reader.GetInt32(4) == (int) ItemCategories.Ship && itemQuantity == 1 && reader.GetBoolean(5) == false)
                        throw new ConCannotTradeNonSingletonShip(this.TypeManager[reader.GetInt32(3)].Name, station.Name);

                    if (reader.GetBoolean(6) == true)
                        throw new ConCannotTradeContraband(this.TypeManager[reader.GetInt32(3)].Name);

                    // quantity MUST match for this operation to succeed
                    if (itemQuantity != quantity)
                        throw new ConCannotTradeItemSanity();

                    ItemQuantityEntry entry = new ItemQuantityEntry()
                    {
                        ItemID = itemID,
                        NodeID = reader.GetInt32(1)
                    };

                    items[itemID] = entry;
                }
            }
            
            // all the items pass the checks, move them in the database
            foreach ((int itemID, ItemQuantityEntry _) in items)
            {
                Database.PrepareQuery(ref connection,
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
    }
}