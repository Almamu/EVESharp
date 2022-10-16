using System;
using System.Collections.Generic;
using System.Data.Common;
using EVESharp.Database.Corporations;

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

    public static CorporationVotes CrpVotesGetType (this IDatabase Database, int voteCaseID)
    {
        return (CorporationVotes) Database.Scalar<int> (
            "CrpVotesGetType",
            new Dictionary <string, object> ()
            {
                {"_voteCaseID", voteCaseID}
            }
        );
    }

    public static uint CrpSharesGet (this IDatabase Database, int ownerID, int corporationID)
    {
        return Database.Scalar<uint> (
            "CrpSharesGet",
            new Dictionary <string, object> ()
            {
                {"_ownerID", ownerID},
                {"_corporationID", corporationID}
            }
        );
    }

    public static void CrpSharesSet (this IDatabase Database, int ownerID, int corporationID, uint shares)
    {
        Database.QueryProcedure (
            "CrpSharesSet",
            new Dictionary <string, object> ()
            {
                {"_ownerID", ownerID},
                {"_corporationID", corporationID},
                {"_shares", shares}
            }
        );
    }

    public static (double, int) CrpVotesGetDecision (this IDatabase Database, int voteCaseID)
    {
        return Database.Scalar<double, int> (
            "CrpVotesGetRate",
            new Dictionary <string, object> ()
            {
                {"_voteCaseID", voteCaseID}
            }
        );
    }

    public static void CrpVotesApply (this IDatabase Database, int voteCaseID)
    {
        Database.QueryProcedure (
            "CrpVotesApply",
            new Dictionary <string, object> ()
            {
                {"_voteCaseID", voteCaseID},
                {"_currentTime", DateTime.Now.ToFileTimeUtc ()}
            }
        );
    }

    public static bool CrpVotesIsExpired (this IDatabase Database, int voteCaseID)
    {
        return 1 == Database.Scalar<int> (
            "CrpVotesIsExpired",
            new Dictionary <string, object> ()
            {
                {"_voteCaseID", voteCaseID},
                {"_currentTime", DateTime.Now.ToFileTimeUtc ()}
            }
        );
    }
}