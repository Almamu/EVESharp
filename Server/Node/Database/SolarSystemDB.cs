using System;
using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;

namespace Node.Database
{
    public class SolarSystemDB : DatabaseAccessor
    {
        public SolarSystemDB(DatabaseConnection db) : base(db)
        {
        }

        public int GetJumpsBetweenSolarSystems(int fromSolarSystemID, int toSolarSystemID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT jumps FROM mapPrecalculatedSolarSystemJumps WHERE fromSolarSystemID = @fromSolarSystemID AND toSolarSystemID = @toSolarSystemID",
                new Dictionary<string, object>()
                {
                    {"@fromSolarSystemID", fromSolarSystemID},
                    {"@toSolarSystemID", toSolarSystemID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    throw new Exception("No route between the given solar systems");

                return reader.GetInt32(0);
            }
        }
    }
}