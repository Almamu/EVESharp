using System.Collections.Generic;
using Common.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.StaticData.Inventory;
using PythonTypes.Types.Database;

namespace Node.Database
{
    public class LookupDB : DatabaseAccessor
    {
        public Rowset LookupStations(string namePart, bool exact)
        {
            if (exact == true)
            {
                return Database.PrepareRowsetQuery(
                    "SELECT stationID, stationName, stationTypeID AS typeID FROM staStations WHERE stationName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    "SELECT stationID, stationName, stationTypeID AS typeID FROM staStations WHERE stationName LIKE @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart + "%"}
                    }
                );
            }
        }
        
        public Rowset LookupCharacters(string namePart, bool exact)
        {
            if (exact == true)
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE groupID = {(int) Groups.Character} AND itemName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE groupID = {(int) Groups.Character} AND itemName LIKE @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart + "%"}
                    }
                );
            }
        }
        
        public Rowset LookupPlayerCharacters(string namePart, bool exact)
        {
            if (exact == true)
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE itemID >= {ItemFactory.USERGENERATED_ID_MIN} AND groupID = {(int) Groups.Character} AND itemName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE itemID >= {ItemFactory.USERGENERATED_ID_MIN} AND groupID = {(int) Groups.Character} AND itemName LIKE @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart + "%"}
                    }
                );
            }
        }

        public Rowset LookupOwners(string namePart, bool exact)
        {
            if (exact == true)
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE categoryID = {(int) Categories.Owner} AND itemName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE categoryID = {(int) Categories.Owner} AND itemName LIKE @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart + "%"}
                    }
                );
            }
        }
        
        public Rowset LookupCorporations(string namePart, bool exact)
        {
            if (exact == true)
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID AS corporationID, itemName AS corporationName, typeID FROM eveNames WHERE itemID >= {ItemFactory.NPC_CORPORATION_ID_MIN} AND groupID = {(int) Groups.Corporation} AND itemName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID AS corporationID, itemName AS corporationName, typeID FROM eveNames WHERE itemID >= {ItemFactory.NPC_CORPORATION_ID_MIN} AND groupID = {(int) Groups.Corporation} AND itemName LIKE @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart + "%"}
                    }
                );
            }
        }
        
        public Rowset LookupCorporationsOrAlliances(string namePart, bool exact)
        {
            if (exact == true)
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE (groupID = {(int) Groups.Corporation} OR groupID = {(int) Groups.Alliance}) AND itemName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE (groupID = {(int) Groups.Corporation} OR groupID = {(int) Groups.Alliance}) AND itemName LIKE @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart + "%"}
                    }
                );
            }
        }
        
        public Rowset LookupWarableCorporationsOrAlliances(string namePart, bool exact)
        {
            if (exact == true)
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE itemID >= {ItemFactory.NPC_CORPORATION_ID_MIN} AND (groupID = {(int) Groups.Corporation} OR groupID = {(int) Groups.Alliance}) AND itemName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID as ownerID, itemName AS ownerName, typeID, groupID FROM eveNames WHERE itemID >= {ItemFactory.NPC_CORPORATION_ID_MIN} AND (groupID = {(int) Groups.Corporation} OR groupID = {(int) Groups.Alliance}) AND itemName LIKE @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart + "%"}
                    }
                );
            }
        }
        
        public Rowset LookupCorporationTickers(string namePart, bool exact)
        {
            if (exact == true)
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT corporationID, corporationName, tickerName, {(int) Types.Corporation} AS typeID FROM corporation WHERE tickerName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT corporationID, corporationName, tickerName, {(int) Types.Corporation} AS typeID FROM corporation WHERE tickerName LIKE @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart + "%"}
                    }
                );
            }
        }
        
        public Rowset LookupKnownLocationsByGroup(string namePart, int groupID)
        {
            return Database.PrepareRowsetQuery(
                $"SELECT itemID, itemName, typeID FROM eveNames WHERE itemID < {ItemFactory.USERGENERATED_ID_MIN} AND groupID = {groupID} AND itemName LIKE @namePart",
                new Dictionary<string, object>()
                {
                    {"@namePart", namePart + "%"}
                }
            );
        }
        
        public Rowset LookupFactions(string namePart, bool exact)
        {
            if (exact == true)
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID AS locationID, itemName AS locationName, typeID FROM eveNames WHERE itemID < {ItemFactory.USERGENERATED_ID_MIN} AND groupID = {(int) Groups.Faction} AND itemName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID AS locationID, itemName AS locationName, typeID FROM eveNames WHERE itemID < {ItemFactory.USERGENERATED_ID_MIN} AND groupID = {(int) Groups.Faction} AND itemName LIKE @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart + "%"}
                    }
                );
            }
        }
        
        public LookupDB(DatabaseConnection db) : base(db)
        {
        }
    }
}