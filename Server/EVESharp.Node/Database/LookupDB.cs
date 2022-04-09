﻿using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Inventory;
using EVESharp.PythonTypes.Types.Database;
using Categories = EVESharp.EVE.StaticData.Inventory.Categories;
using Groups = EVESharp.EVE.StaticData.Inventory.Groups;

namespace EVESharp.Node.Database;

public class LookupDB : DatabaseAccessor
{
    public LookupDB (DatabaseConnection db) : base (db) { }

    public Rowset LookupStations (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                "SELECT stationID, stationName, stationTypeID AS typeID FROM staStations WHERE stationName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            "SELECT stationID, stationName, stationTypeID AS typeID FROM staStations WHERE stationName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupCharacters (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE groupID = {(int) Groups.Character} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE groupID = {(int) Groups.Character} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupPlayerCharacters (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE itemID >= {ItemRanges.USERGENERATED_ID_MIN} AND groupID = {(int) Groups.Character} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE itemID >= {ItemRanges.USERGENERATED_ID_MIN} AND groupID = {(int) Groups.Character} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupOwners (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE categoryID = {(int) Categories.Owner} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE categoryID = {(int) Categories.Owner} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupCorporations (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                $"SELECT itemID AS corporationID, itemName AS corporationName, typeID FROM eveNames WHERE itemID >= {ItemRanges.NPC_CORPORATION_ID_MIN} AND groupID = {(int) Groups.Corporation} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            $"SELECT itemID AS corporationID, itemName AS corporationName, typeID FROM eveNames WHERE itemID >= {ItemRanges.NPC_CORPORATION_ID_MIN} AND groupID = {(int) Groups.Corporation} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupAlliances (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                $"SELECT itemID AS allianceID, itemName AS allianceName, typeID FROM eveNames WHERE itemID >= {ItemRanges.NPC_CORPORATION_ID_MIN} AND groupID = {(int) Groups.Alliance} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            $"SELECT itemID AS allianceID, itemName AS allianceName, typeID FROM eveNames WHERE itemID >= {ItemRanges.NPC_CORPORATION_ID_MIN} AND groupID = {(int) Groups.Alliance} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupAllianceShortNames (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                $"SELECT allianceID, itemName AS allianceName, {(int) Types.Alliance} AS typeID LEFT JOIN eveNames ON allianceID = itemID FROM crpAlliances WHERE shortName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            $"SELECT allianceID, itemName AS allianceName, {(int) Types.Alliance} AS typeID LEFT JOIN eveNames ON allianceID = itemID FROM crpAlliances WHERE shortName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupCorporationsOrAlliances (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE (groupID = {(int) Groups.Corporation} OR groupID = {(int) Groups.Alliance}) AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE (groupID = {(int) Groups.Corporation} OR groupID = {(int) Groups.Alliance}) AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupWarableCorporationsOrAlliances (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE itemID >= {ItemRanges.NPC_CORPORATION_ID_MIN} AND (groupID = {(int) Groups.Corporation} OR groupID = {(int) Groups.Alliance}) AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE itemID >= {ItemRanges.NPC_CORPORATION_ID_MIN} AND (groupID = {(int) Groups.Corporation} OR groupID = {(int) Groups.Alliance}) AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupCorporationTickers (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                $"SELECT corporationID, corporationName, tickerName, {(int) Types.Corporation} AS typeID FROM corporation WHERE tickerName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            $"SELECT corporationID, corporationName, tickerName, {(int) Types.Corporation} AS typeID FROM corporation WHERE tickerName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupKnownLocationsByGroup (string namePart, int groupID)
    {
        return Database.PrepareRowsetQuery (
            $"SELECT itemID, itemName, typeID FROM eveNames WHERE itemID < {ItemRanges.USERGENERATED_ID_MIN} AND groupID = {groupID} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupFactions (string namePart, bool exact)
    {
        if (exact)
            return Database.PrepareRowsetQuery (
                $"SELECT itemID AS locationID, itemName AS locationName, typeID FROM eveNames WHERE itemID < {ItemRanges.USERGENERATED_ID_MIN} AND groupID = {(int) Groups.Faction} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return Database.PrepareRowsetQuery (
            $"SELECT itemID AS locationID, itemName AS locationName, typeID FROM eveNames WHERE itemID < {ItemRanges.USERGENERATED_ID_MIN} AND groupID = {(int) Groups.Faction} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }
}