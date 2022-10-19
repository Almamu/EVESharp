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
using System.Data.Common;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Inventory.Categories;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Market;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions.contractMgr;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Type = System.Type;

namespace EVESharp.Database.Old;

public class ContractDB : DatabaseAccessor
{
    private ITypes Types { get; }

    public ContractDB (ITypes types, IDatabase db) : base (db)
    {
        this.Types = types;
    }

    public int GetOutstandingContractsCountForPlayer (int characterID)
    {
        DbDataReader reader = this.Database.Select (
            "SELECT COUNT(*) AS contractCount FROM conContracts WHERE issuerID = @characterID and forCorp = @forCorp AND status = @outstandingStatus AND dateExpired > @currentTime",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@forCorp", 0},
                {"@outstandingStatus", ContractStatus.Outstanding},
                {"@currentTime", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );

        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt32 (0);
        }
    }

    public PyDataType NumRequiringAttention (int characterID, int corporationID)
    {
        return this.Database.PrepareKeyVal (
            "SELECT" +
            " (SELECT COUNT(*) FROM conContracts WHERE forCorp = @notForCorp AND ((issuerID = @characterID AND dateExpired < @currentTime) OR (acceptorID = @characterID AND dateCompleted <= @currentTime))) AS n," +
            " (SELECT COUNT(*) FROM conContracts WHERE forCorp = @forCorp AND ((issuerCorpID = @corporationID AND dateExpired < @currentTime) OR (acceptorID = @corporationID AND dateCompleted <= @currentTime))) AS ncorp",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID},
                {"@forCorp", 1},
                {"@notForCorp", 0},
                {"@currentTime", DateTime.Now.ToFileTimeUtc ()}
            }
        );
    }

    public PyDataType NumOutstandingContracts (int characterID, int corporationID)
    {
        return this.Database.PrepareKeyVal (
            "SELECT" +
            " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @corporationID AND forCorp = @forCorp AND status = @outstandingStatus AND dateExpired > @currentTime) AS myCorpTotal," +
            " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @characterID AND forCorp = @notForCorp AND status = @outstandingStatus AND dateExpired > @currentTime) AS myCharTotal," +
            " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID AND forCorp = @notForCorp AND status = @outstandingStatus AND dateExpired > @currentTime) AS nonCorpForMyCorp," +
            " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID AND forCorp = @notForCorp AND status = @outstandingStatus AND dateExpired > @currentTime) AS nonCorpForMyChar",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID},
                {"@forCorp", 1},
                {"@notForCorp", 0},
                {"@outstandingStatus", (int) ContractStatus.Outstanding},
                {"@currentTime", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public PyDataType CollectMyPageInfo (int characterID, int corporationID)
    {
        // TODO: PROPERLY IMPLEMENT THIS
        return this.Database.PrepareKeyVal (
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
            new Dictionary <string, object>
            {
                {"@outstandingStatus", (int) ContractStatus.Outstanding},
                {"@inProgressStatus", (int) ContractStatus.InProgress},
                {"@notForCorp", 0},
                {"@forCorp", 1},
                {"@corporationID", corporationID},
                {"@characterID", characterID},
                {"@currentTime", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public Rowset GetItemsInStationForPlayer (int characterID, int stationID)
    {
        return this.Database.PrepareRowset (
            "SELECT itemID, typeID, categoryID, groupID, singleton, quantity, flag, contraband FROM invItems LEFT JOIN invTypes USING (typeID) LEFT JOIN invGroups USING (groupID) WHERE ownerID = @characterID AND locationID = @stationID AND flag = @flagHangar",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@stationID", stationID},
                {"@flagHangar", Flags.Hangar}
            }
        );
    }

    public ulong CreateContract
    (
        int    characterID, int    corporationID, int?   allianceID,     ContractTypes type,         int    availability,
        int           assigneeID, int    expireTime,  int    duration,      int    startStationID, int?          endStationID, double price,
        double        reward,     double collateral,  string title,         string description,    int           issuerWalletID
    )
    {
        return this.Database.Insert (
            "INSERT INTO conContracts(issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, endStationID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, issuerWalletKey, issuerAllianceID, acceptorWalletKey)VALUES(@issuerID, @issuerCorpID, @type, @availability, @assigneeID, @expiretime, @numDays, @startStationID, @endStationID, @price, @reward, @collateral, @title, @description, @forCorp, @status, @isAccepted, @acceptorID, @dateIssued, @dateExpired, @dateAccepted, @dateCompleted, @issuerWalletKey, @issuerAllianceID, @acceptorWalletKey)",
            new Dictionary <string, object>
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
                {"@dateExpired", DateTime.UtcNow.AddMinutes (expireTime).ToFileTimeUtc ()},
                {"@dateAccepted", 0},
                {"@dateCompleted", 0},
                {"@issuerWalletKey", issuerWalletID},
                {"@issuerAllianceID", allianceID},
                {"@acceptorWalletKey", null}
            }
        );
    }

    public PyPackedRow GetContractInformation (int contractID, int characterID, int corporationID)
    {
        return this.Database.PreparePackedRow (
            "SELECT contractID, issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, start.solarSystemID AS startSolarSystemID, start.regionID AS startRegionID, endStationID, end.solarSystemID AS endSolarSystemID, end.regionID AS endRegionID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, volume, crateID, issuerWalletKey, issuerAllianceID, acceptorWalletKey FROM conContracts LEFT JOIN staStations AS start ON start.stationID = startStationID LEFT JOIN staStations AS end ON end.stationID = endStationID WHERE ((availability = 1 AND (issuerID = @characterID OR issuerCorpID = @corporationID OR assigneeID = @characterID OR assigneeID = @corporationID OR acceptorID = @characterID OR acceptorID = @corporationID)) OR availability = 0) AND contractID = @contractID",
            new Dictionary <string, object>
            {
                {"@contractID", contractID},
                {"@characterID", characterID},
                {"@corporationID", corporationID}
            }
        );
    }

    public Rowset GetContractBids (int contractID, int characterID, int corporationID)
    {
        return this.Database.PrepareRowset (
            "SELECT bidID, bidderID, amount FROM conBids WHERE contractID = @contractID ORDER BY amount DESC",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID},
                {"@contractID", contractID}
            }
        );
    }

    public Rowset GetContractItems (int contractID, int characterID, int corporationID)
    {
        return this.Database.PrepareRowset (
            "SELECT itemTypeID AS typeID, quantity, inCrate, itemID, materialLevel, productivityLevel, licensedProductionRunsRemaining AS bpRuns FROM conItems LEFT JOIN invBlueprints USING(itemID) WHERE contractID = @contractID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID},
                {"@contractID", contractID}
            }
        );
    }

    public PyList <PyPackedRow> GetItemsInContainer (int characterID, int containerID)
    {
        return this.Database.PreparePackedRowList (
            "SELECT itemID, typeID, quantity FROM invItems WHERE locationID = @containerID",
            new Dictionary <string, object> {{"@containerID", containerID}}
        );
    }

    public List <int> GetContractList
    (
        int? startContractID, int  limit, int? itemTypeID, PyList <PyInteger> notIssuedByIDs, PyList <PyInteger> issuedByIDs, int? assigneeID, int? locationID,
        int? itemGroupID,     int? itemCategoryID, int priceMax, int priceMin, int? type, string description, int callerID,
        int  callerCorpID,    int? ownerID = null, int? status = null, bool includeExpired = false, bool expiredOnly = false
    )
    {
        string contractQuery =
            "SELECT contractID FROM conContracts LEFT JOIN conItems USING(contractID) LEFT JOIN invTypes ON conItems.itemTypeID = invTypes.typeID LEFT JOIN invGroups USING(groupID) LEFT JOIN staStations ON staStations.stationID = conContracts.startStationID WHERE (availability = 0 OR (availability = 1 AND issuerCorpID = @corporationID) OR assigneeID = @characterID)";

        Dictionary <string, object> values = new Dictionary <string, object> ();

        if (startContractID > 0)
        {
            contractQuery               += " AND contractID < @startContractID";
            values ["@startContractID"] =  startContractID;
        }

        if (itemTypeID is not null)
        {
            contractQuery          += " AND itemTypeID = @itemTypeID";
            values ["@itemTypeID"] =  itemTypeID;
        }

        if (assigneeID is not null)
        {
            contractQuery          += " AND assigneeID = @assigneeID";
            values ["@assigneeID"] =  assigneeID;
        }

        if (locationID is not null)
        {
            contractQuery          += " AND (startStationID = @locationID OR solarSystemID = @locationID OR regionID = @locationID)";
            values ["@locationID"] =  locationID;
        }

        if (notIssuedByIDs is not null && notIssuedByIDs.Count > 0)
            contractQuery += " AND ((issuerID NOT IN (" + string.Join <PyInteger> (',', notIssuedByIDs) + ") AND forCorp = 0) AND (issuerCorpID NOT IN (" +
                             string.Join <PyInteger> (',',                              notIssuedByIDs) + ") AND forCorp = 1))";

        if (issuedByIDs is not null && issuedByIDs.Count > 0)
            contractQuery += " AND ((issuerID IN (" + string.Join <PyInteger> (',', issuedByIDs) + ") AND forCorp = 0) OR (issuerCorpID IN (" +
                             string.Join <PyInteger> (',',                          issuedByIDs) + ") AND forCorp = 1))";

        if (itemCategoryID is not null)
        {
            contractQuery              += " AND categoryID = @itemCategoryID";
            values ["@itemCategoryID"] =  itemCategoryID;
        }

        if (itemGroupID is not null)
        {
            contractQuery           += " AND groupID = @itemGroupID";
            values ["@itemGroupID"] =  itemGroupID;
        }

        if (priceMax > 0)
        {
            contractQuery        += " AND price < @priceMax";
            values ["@priceMax"] =  priceMax;
        }

        if (priceMin > 0)
        {
            contractQuery        += " AND price > @priceMin";
            values ["@priceMin"] =  priceMin;
        }

        if (type is not null)
        {
            if (type == 10)
            {
                contractQuery     += " AND (type = @type1 OR type = @type2)";
                values ["@type1"] =  (int) ContractTypes.ItemExchange;
                values ["@type2"] =  (int) ContractTypes.Auction;
            }
            else
            {
                contractQuery    += " AND type = @type";
                values ["@type"] =  type;
            }
        }

        if (description is not null)
        {
            contractQuery           += " AND description LIKE @description";
            values ["@description"] =  $"%{description}%";
        }

        if (includeExpired == false)
        {
            contractQuery           += " AND dateExpired > @currentTime";
            values ["@currentTime"] =  DateTime.UtcNow.ToFileTimeUtc ();
        }

        if (expiredOnly)
        {
            contractQuery           += " AND dateExpired <= @currentTime";
            values ["@currentTime"] =  DateTime.UtcNow.ToFileTimeUtc ();
        }

        if (ownerID is not null)
        {
            contractQuery       += " AND ((issuerID = @ownerID AND forCorp = 0) OR (issuerCorpID = @ownerID AND forCorp = 1))";
            values ["@ownerID"] =  ownerID;
        }

        if (status is not null)
        {
            contractQuery      += " AND status = @status";
            values ["@status"] =  status;
        }

        // visibility checks are here too!
        values ["@characterID"]   = callerID;
        values ["@corporationID"] = callerCorpID;

        if (limit > 0)
            contractQuery += $" LIMIT {limit}";

        using (DbDataReader reader = this.Database.Select (contractQuery, values))
        {
            List <int> result = new List <int> ();

            while (reader.Read ())
                result.Add (reader.GetInt32 (0));

            // ensure that there's at least one id in the list so any fetch queries do not fail
            if (result.Count == 0)
                result.Add (0);

            return result;
        }
    }

    public List <int> GetContractListByOwnerBids (int ownerID)
    {
        DbDataReader reader = this.Database.Select (
            "SELECT contractID FROM conBids WHERE bidderID = @ownerID",
            new Dictionary <string, object> {{"@ownerID", ownerID}}
        );

        using (reader)
        {
            List <int> result = new List <int> ();

            while (reader.Read ())
                result.Add (reader.GetInt32 (0));

            // ensure that there's at least one id in the list so any fetch queries do not fail
            if (result.Count == 0)
                result.Add (0);

            return result;
        }
    }

    public List <int> GetContractListByAcceptor (int acceptorID)
    {
        DbDataReader reader = this.Database.Select (
            "SELECT contractID FROM conContracts WHERE acceptorID = @acceptorID AND status = @status",
            new Dictionary <string, object>
            {
                {"@acceptorID", acceptorID},
                {"@status", (int) ContractStatus.InProgress}
            }
        );

        using (reader)
        {
            List <int> result = new List <int> ();

            while (reader.Read ())
                result.Add (reader.GetInt32 (0));

            // ensure that there's at least one id in the list so any fetch queries do not fail
            if (result.Count == 0)
                result.Add (0);

            return result;
        }
    }

    public PyDataType GetInformationForContractList (List <int> contractList)
    {
        return this.Database.PrepareCRowset (
            $"SELECT contractID, issuerID, issuerCorpID, type, availability, assigneeID, expiretime, numDays, startStationID, start.solarSystemID AS startSolarSystemID, start.regionID AS startRegionID, endStationID, end.solarSystemID AS endSolarSystemID, end.regionID AS endRegionID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, volume, crateID, issuerWalletKey, issuerAllianceID, acceptorWalletKey FROM conContracts LEFT JOIN staStations AS start ON start.stationID = startStationID LEFT JOIN staStations AS end ON end.stationID = endStationID WHERE contractID IN ({string.Join (',', contractList)})"
        );
    }

    public PyDataType GetBidsForContractList (List <int> contractList)
    {
        return this.Database.PrepareIntPackedRowListDictionary (
            $"SELECT contractID, amount, bidderID FROM conBids WHERE contractID IN ({string.Join (',', contractList)}) ORDER BY contractID, amount DESC",
            0
        );
    }

    public PyDataType GetItemsForContractList (List <int> contractList)
    {
        return this.Database.PrepareIntPackedRowListDictionary (
            $"SELECT contractID, itemTypeID AS typeID, quantity, inCrate, materialLevel, productivityLevel, licensedProductionRunsRemaining AS bpRuns FROM conItems LEFT JOIN invBlueprints USING(itemID) WHERE contractID IN ({string.Join (',', contractList)}) ORDER BY contractID",
            0
        );
    }

    public List <int> FetchLoginCharacterContractBids (int bidderID)
    {
        DbDataReader reader = this.Database.Select (
            "SELECT contractID, MAX(amount), (SELECT MAX(amount) FROM conBids b WHERE b.contractID = contractID) AS maximum FROM conBids LEFT JOIN conContracts USING(contractID) WHERE bidderID = @bidderID AND status = @outstandingStatus AND dateExpired < @currentTime GROUP BY contractID",
            new Dictionary <string, object>
            {
                {"@bidderID", bidderID},
                {"@outstandingStatus", ContractStatus.Outstanding},
                {"@currentTime", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );

        using (reader)
        {
            List <int> result = new List <int> ();

            while (reader.Read ())
                if (reader.GetInt32 (1) != reader.GetInt32 (2))
                    result.Add (reader.GetInt32 (0));

            return result;
        }
    }

    public List <int> FetchLoginCharacterContractAssigned (int assigneeID)
    {
        DbDataReader reader = this.Database.Select (
            "SELECT contractID FROM conContracts WHERE assigneeID = @assigneeID AND status = @outstandingStatus AND dateExpired < @currentTime",
            new Dictionary <string, object>
            {
                {"@assigneeID", assigneeID},
                {"@outstandingStatus", ContractStatus.Outstanding},
                {"@currentTime", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );

        using (reader)
        {
            List <int> result = new List <int> ();

            while (reader.Read ())
                result.Add (reader.GetInt32 (0));

            return result;
        }
    }
}