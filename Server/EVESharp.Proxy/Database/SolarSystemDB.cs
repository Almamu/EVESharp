using System.Collections.Generic;
using EVESharp.Common.Database;
using MySql.Data.MySqlClient;

namespace EVESharp.Proxy.Database
{
    public class SolarSystemDB : DatabaseAccessor
    {
        public const int SOLARSYSTEM_ID_MIN = 30000000;
        public const int SOLARSYSTEM_ID_MAX = 40000000;
        
        public int GetSolarSystemNodeID(int solarSystemID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT nodeID FROM invItems WHERE itemID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@itemID", solarSystemID}
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

        public void SetSolarSystemNodeID(int solarSystemID, long nodeID)
        {
            Database.PrepareQuery(
                "UPDATE invItems SET nodeID = @nodeID WHERE itemID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@nodeID", nodeID},
                    {"@itemID", solarSystemID}
                }
            );
        }

        public void ClearSolarSystemNodeID()
        {
            Database.Query("UPDATE invItems SET nodeID = 0");
        }
        
        public SolarSystemDB(DatabaseConnection db) : base(db)
        {
        }
    }
}