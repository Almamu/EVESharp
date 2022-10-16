using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EVESharp.Database.Extensions;

public static class StationsDB
{
    public static uint? CrpOfficeGetAtStation (this IDatabase Database, int corporationID, int stationID)
    {
        return Database.Scalar<uint?> (
            "CrpOfficeGetAtStation",
            new Dictionary <string, object> ()
            {
                {"_corporationID", corporationID},
                {"_stationID", stationID}
            }
        );
    }

    public static void CrpOfficeDestroyOrImpound (this IDatabase Database, int officeFolderID)
    {
        Database.QueryProcedure (
            "CrpOfficeDestroyOrImpound",
            new Dictionary <string, object> ()
            {
                {"_officeFolderID", officeFolderID}
            }
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Database"></param>
    /// <returns>stationID, corporationID, officeFolderID</returns>
    public static IEnumerable <(int, int, int)> CrpOfficesGetExpired (this IDatabase Database)
    {
        DbDataReader reader = Database.SelectProcedure(
            "CrpOfficesGetExpired",
            new Dictionary <string, object> ()
            {
                {"_currentTime", DateTime.Now.ToFileTimeUtc ()}
            }
        );

        using (reader)
        {
            while (reader.Read () == true)
            {
                yield return (reader.GetInt32 (0), reader.GetInt32 (1), reader.GetInt32 (2));
            }
        }
    }
}