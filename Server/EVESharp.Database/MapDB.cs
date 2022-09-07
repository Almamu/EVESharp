using System.Collections.Generic;
using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Database;

public static class MapDB
{
    public static int MapCalculateJumps (this IDatabaseConnection Database, int fromSolarSystemID, int toSolarSystemID)
    {
        if (fromSolarSystemID == toSolarSystemID)
            return 0;
        
        return Database.Scalar<int> (
            "MapCalculateJumps",
            new Dictionary<string, object>
            {
                {"_fromSolarSystemID", fromSolarSystemID},
                {"_toSolarSystemID", toSolarSystemID}
            }
        );
    }
}