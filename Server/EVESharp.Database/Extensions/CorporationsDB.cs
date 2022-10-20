using System;
using System.Collections.Generic;
using System.Data.Common;
using EVESharp.Database.Corporations;
using EVESharp.Types;
using EVESharp.Types.Collections;

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
    
    public static IEnumerable<(int corporationID, int voteCaseID, int parameter, int voteType, double rate)> CrpVotesGetAffectedByHousekeeping (this IDatabase Database, long currentTime)
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
            while (reader.Read () == true)
                yield return (
                    reader.GetInt32 (0),
                    reader.GetInt32 (1),
                    reader.GetInt32 (2),
                    reader.GetInt32 (3),
                    reader.GetDouble (4)
                );
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

    public static int CrpVotesGetCorporation (this IDatabase Database, int voteCaseID)
    {
        return Database.Scalar <int> (
            "CrpVotesGetCorporation",
            new Dictionary<string, object> ()
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

    public static bool CrpVotesHasEnded (this IDatabase Database, int voteCaseID)
    {
        return 1 == Database.Scalar <int> (
            "CrpVotesHasEnded",
            new Dictionary <string, object> ()
            {
                {"_voteCaseID", voteCaseID},
                {"_currentTime", DateTime.Now.ToFileTimeUtc ()}
            }
        );
    }

    public static bool CrpVotesExists (this IDatabase Database, int voteCaseID)
    {
        return 1 == Database.Scalar <int> (
            "CrpVotesExists",
            new Dictionary<string, object> ()
            {
                {"_voteCaseID", voteCaseID}
            }
        );
    }

    public static bool CrpVotesHasVoted (this IDatabase Database, int voteCaseID, int characterID)
    {
        return 0 < Database.Scalar<int> (
            "CrpVotesHasVoted",
            new Dictionary <string, object> ()
            {
                {"_voteCaseID", voteCaseID},
                {"_characterID", characterID}
            }
        );
    }

    public static PyList <PyInteger> InvItemsLockedGetLocations (this IDatabase Database, int corporationID)
    {
        return Database.List<PyInteger> (
            "InvItemsLockedGetLocations",
            new Dictionary <string, object> ()
            {
                {"_corporationID", corporationID}
            }
        );
    }

    public static PyDataType InvItemsLockedGetAtLocation (this IDatabase Database, int corporationID, int stationID)
    {
        return Database.IndexRowset (
            0, "InvItemsLockedGetAtLocation",
            new Dictionary <string, object> ()
            {
                {"_corporationID", corporationID},
                {"_stationID", stationID}
            }
        );
    }

    public static void InvItemsLockedAdd (this IDatabase Database, int itemID, int corporationID, int stationID, int voteCaseID)
    {
        Database.QueryProcedure (
            "InvItemsLockedAdd",
            new Dictionary <string, object> ()
            {
                {"_itemID", itemID},
                {"_corporationID", corporationID},
                {"_stationID", stationID},
                {"_voteCaseID", voteCaseID}
            }
        );
    }

    public static (int itemID, int corporationID, int stationID) InvItemsLockedRemove (this IDatabase Database, int voteCaseID)
    {
        return Database.Scalar<int, int, int> (
            "InvItemsLockedRemove",
            new Dictionary <string, object> ()
            {
                {"_voteCaseID", voteCaseID}
            }
        );
    }

    public static (int corporationID, int stationID) InvItemsLockedRemoveByID (this IDatabase Database, int itemID)
    {
        return Database.Scalar<int, int> (
            "InvItemsLockedRemoveByID",
            new Dictionary <string, object> ()
            {
                {"_itemID", itemID}
            }
        );
    }

    public static PyDictionary <PyString, PyTuple> CrpVotesGetAsSanctionable (this IDatabase Database, int voteCaseID)
    {
        return Database.DifferenceDict (
            "CrpVotesGetAsSanctionable",
            new Dictionary <string, object> ()
            {
                {"_voteCaseID", voteCaseID}
            }
        );
    }

    public static bool CrpOfficesGetAtStation (this IDatabase Database, int corporationID, int stationID, bool impounded, out int officeFolderID)
    {
        DbDataReader reader = Database.SelectProcedure (
            "CrpOfficesGetAtStation",
            new Dictionary <string, object> ()
            {
                {"_corporationID", corporationID},
                {"_stationID", stationID},
                {"_impounded", impounded}
            }
        );

        officeFolderID = 0;

        using (reader)
        {
            if (reader.Read () == false)
                return false;

            officeFolderID = reader.GetInt32 (0);

            return true;
        }
    }
}