using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EVESharp.Database.Extensions;

public static class CorporationsDB
{
    public static void CrpVoteHousekeeping (this IDatabase Database, long currentTime)
    {
        Database.QueryProcedure (
            "CrpVoteHousekeeping",
            new Dictionary <string, object> ()
            {
                {"_currentTime", currentTime}
            }
        );
    }
    
    public static void CrpAdsHousekeeping (this IDatabase Database, long currentTime)
    {
        Database.QueryProcedure (
            "CrpAdsHousekeeping",
            new Dictionary <string, object> ()
            {
                {"_currentTime", currentTime}
            }
        );
    }
    
    public static List<KeyValuePair<int,ulong>> CrpAdsGetAffectedByHousekeeping (this IDatabase Database, long currentTime)
    {
        DbDataReader reader = Database.SelectProcedure (
            "CrpAdsGetAffectedByHousekeeping",
            new Dictionary <string, object> ()
            {
                {"_currentTime", currentTime}
            }
        );

        using (reader)
        {
            List <KeyValuePair<int, ulong>> result = new List <KeyValuePair<int, ulong>> ();

            while (reader.Read () == true)
                result.Add (new KeyValuePair<int, ulong> (reader.GetInt32 (0), (ulong) reader.GetInt64 (1)));

            return result;
        }
    }
    
    public static List<KeyValuePair<int,int>> CrpVotesGetAffectedByHousekeeping (this IDatabase Database, long currentTime)
    {
        DbDataReader reader = Database.SelectProcedure (
            "CrpVotesGetAffectedByHousekeeping",
            new Dictionary <string, object> ()
            {
                {"_currentTime", currentTime}
            }
        );

        using (reader)
        {
            List <KeyValuePair<int, int>> result = new List <KeyValuePair<int, int>> ();

            while (reader.Read () == true)
                result.Add (new KeyValuePair<int, int> (reader.GetInt32 (0), reader.GetInt32 (1)));

            return result;
        }
    }
}