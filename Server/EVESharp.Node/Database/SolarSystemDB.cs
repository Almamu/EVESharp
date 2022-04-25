using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.PythonTypes.Types.Database;
using MySql.Data.MySqlClient;

namespace EVESharp.Node.Database;

public class SolarSystemDB : DatabaseAccessor
{
    public SolarSystemDB (IDatabaseConnection db) : base (db) { }

    public int GetJumpsBetweenSolarSystems (int fromSolarSystemID, int toSolarSystemID)
    {
        if (fromSolarSystemID == toSolarSystemID)
            return 0;

        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT jumps FROM mapPrecalculatedSolarSystemJumps WHERE fromSolarSystemID = @fromSolarSystemID AND toSolarSystemID = @toSolarSystemID",
            new Dictionary <string, object>
            {
                {"@fromSolarSystemID", fromSolarSystemID},
                {"@toSolarSystemID", toSolarSystemID}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                throw new Exception ("No route between the given solar systems");

            return reader.GetInt32 (0);
        }
    }
}