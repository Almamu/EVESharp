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
                    $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE itemID >= {ItemManager.USERGENERATED_ID_MIN} AND groupID = {(int) Groups.Character} AND itemName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    $"SELECT itemID AS characterID, itemName AS characterName, typeID FROM eveNames WHERE itemID >= {ItemManager.USERGENERATED_ID_MIN} AND groupID = {(int) Groups.Character} AND itemName LIKE @namePart",
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
        
        public LookupDB(DatabaseConnection db) : base(db)
        {
        }
    }
}