using System.Collections.Generic;

namespace EVESharp.Database.Extensions;

public static class MapDB
{
    public static int MapCalculateJumps (this IDatabase Database, int fromSolarSystemID, int toSolarSystemID)
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