using System.Collections.Generic;
using Common.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class StandingDB : DatabaseAccessor
    {
        public StandingDB(DatabaseConnection db) : base(db)
        {
        }

        public PyDataType GetCharStandings(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT toID, standing FROM chrStandings WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public PyDataType GetCharPrime(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT itemID as ownerID, itemName as ownerName, typeID FROM chrStandings, entity WHERE characterID = @characterID AND entity.itemID = chrStandings.toID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public PyDataType GetCharNPCStandings(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT fromID, standing FROM chrNPCStandings WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }
    }
}