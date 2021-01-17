using System.Collections.Generic;
using Common.Database;
using Node.Inventory;
using PythonTypes.Types.Database;

namespace Node.Services.Data
{
    public class LookupDB : DatabaseAccessor
    {
        public Rowset LookupPlayerCharacters(string namePart, bool exact)
        {
            if (exact == true)
            {
                return Database.PrepareRowsetQuery(
                    "SELECT characterID, itemName AS characterName, typeID " +
                    "FROM chrInformation " +
                    "LEFT JOIN entity ON characterID = itemID " +
                    $"WHERE characterID >= {ItemManager.USERGENERATED_ID_MIN} AND itemName = @namePart",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    "SELECT characterID, itemName AS characterName, typeID " +
                    "FROM chrInformation " +
                    "LEFT JOIN entity ON characterID = itemID " +
                    $"WHERE characterID >= {ItemManager.USERGENERATED_ID_MIN} AND itemName LIKE @namePart",
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
                    "SELECT characterID AS ownerID, itemName AS ownerName, typeID " +
                    "FROM chrInformation " +
                    "LEFT JOIN entity ON characterID = itemID " +
                    $"WHERE itemName = @namePart AND characterID >= {ItemManager.USERGENERATED_ID_MIN} " +
                    "UNION " +
                    "SELECT corporationID AS ownerID, itemName AS ownerName, typeID " +
                    "FROM corporation " +
                    "LEFT JOIN entity ON corporationID = itemID " +
                    $"WHERE itemName = @namePart AND corporationID >= {ItemManager.USERGENERATED_ID_MIN}",
                    new Dictionary<string, object>()
                    {
                        {"@namePart", namePart}
                    }
                );                
            }
            else
            {
                return Database.PrepareRowsetQuery(
                    "SELECT characterID AS ownerID, itemName AS ownerName, typeID " +
                    "FROM chrInformation " +
                    "LEFT JOIN entity ON characterID = itemID " +
                    $"WHERE itemName LIKE @namePart AND characterID >= {ItemManager.USERGENERATED_ID_MIN} " +
                    "UNION " +
                    "SELECT corporationID AS ownerID, itemName AS ownerName, typeID " +
                    "FROM corporation " +
                    "LEFT JOIN entity ON corporationID = itemID " +
                    $"WHERE itemName LIKE @namePart AND corporationID >= {ItemManager.USERGENERATED_ID_MIN}",
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