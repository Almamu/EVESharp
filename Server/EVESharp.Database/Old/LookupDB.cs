using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Types;

namespace EVESharp.Database.Old;

public class LookupDB : DatabaseAccessor
{
    public LookupDB (IDatabaseConnection db) : base (db) { }

    public Rowset LookupStations (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                "SELECT stationID, stationName, stationTypeID AS typeID FROM staStations WHERE stationName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            "SELECT stationID, stationName, stationTypeID AS typeID FROM staStations WHERE stationName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupCharacters (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE groupID = {(int) GroupID.Character} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE groupID = {(int) GroupID.Character} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupPlayerCharacters (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE itemID >= {ItemRanges.UserGenerated.MIN} AND groupID = {(int) GroupID.Character} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE itemID >= {ItemRanges.UserGenerated.MIN} AND groupID = {(int) GroupID.Character} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupOwners (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE categoryID = {(int) CategoryID.Owner} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE categoryID = {(int) CategoryID.Owner} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupCorporations (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                $"SELECT itemID AS corporationID, itemName AS corporationName, typeID FROM eveNames WHERE itemID >= {ItemRanges.NPCCorporations.MIN} AND groupID = {(int) GroupID.Corporation} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            $"SELECT itemID AS corporationID, itemName AS corporationName, typeID FROM eveNames WHERE itemID >= {ItemRanges.NPCCorporations.MIN} AND groupID = {(int) GroupID.Corporation} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupAlliances (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                $"SELECT itemID AS allianceID, itemName AS allianceName, typeID FROM eveNames WHERE itemID >= {ItemRanges.UserGenerated.MIN} AND groupID = {(int) GroupID.Alliance} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            $"SELECT itemID AS allianceID, itemName AS allianceName, typeID FROM eveNames WHERE itemID >= {ItemRanges.UserGenerated.MIN} AND groupID = {(int) GroupID.Alliance} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupAllianceShortNames (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                $"SELECT allianceID, itemName AS allianceName, {(int) TypeID.Alliance} AS typeID LEFT JOIN eveNames ON allianceID = itemID FROM crpAlliances WHERE shortName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            $"SELECT allianceID, itemName AS allianceName, {(int) TypeID.Alliance} AS typeID LEFT JOIN eveNames ON allianceID = itemID FROM crpAlliances WHERE shortName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupCorporationsOrAlliances (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE (groupID = {(int) GroupID.Corporation} OR groupID = {(int) GroupID.Alliance}) AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE (groupID = {(int) GroupID.Corporation} OR groupID = {(int) GroupID.Alliance}) AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupWarableCorporationsOrAlliances (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE itemID >= {ItemRanges.NPCCorporations.MIN} AND (groupID = {(int) GroupID.Corporation} OR groupID = {(int) GroupID.Alliance}) AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE itemID >= {ItemRanges.NPCCorporations.MIN} AND (groupID = {(int) GroupID.Corporation} OR groupID = {(int) GroupID.Alliance}) AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupCorporationTickers (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                $"SELECT corporationID, corporationName, tickerName, {(int) TypeID.Corporation} AS typeID FROM corporation WHERE tickerName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            $"SELECT corporationID, corporationName, tickerName, {(int) TypeID.Corporation} AS typeID FROM corporation WHERE tickerName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupKnownLocationsByGroup (string namePart, int groupID)
    {
        return this.Database.PrepareRowset (
            $"SELECT itemID, itemName, typeID FROM eveNames WHERE itemID < {ItemRanges.UserGenerated.MIN} AND groupID = {groupID} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }

    public Rowset LookupFactions (string namePart, bool exact)
    {
        if (exact)
            return this.Database.PrepareRowset (
                $"SELECT itemID AS locationID, itemName AS locationName, typeID FROM eveNames WHERE itemID < {ItemRanges.UserGenerated.MIN} AND groupID = {(int) GroupID.Faction} AND itemName = @namePart",
                new Dictionary <string, object> {{"@namePart", namePart}}
            );

        return this.Database.PrepareRowset (
            $"SELECT itemID AS locationID, itemName AS locationName, typeID FROM eveNames WHERE itemID < {ItemRanges.UserGenerated.MIN} AND groupID = {(int) GroupID.Faction} AND itemName LIKE @namePart",
            new Dictionary <string, object> {{"@namePart", namePart + "%"}}
        );
    }
}