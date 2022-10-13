using System;
using System.Collections.Generic;
using EVESharp.Database.Types;

namespace EVESharp.Database.Extensions;

public static class AlliancesDB
{
    public static void CrpAlliancesCreate (this IDatabase Database, int allianceID, string shortName, string description, string url, int creatorID, int creatorCharacterID)
    {
        Database.QueryProcedure (
            "CrpAlliancesCreate",
            new Dictionary <string, object>
            {
                {"_allianceID", allianceID},
                {"_shortName", shortName},
                {"_description", description},
                {"_url", url},
                {"_creatorID", creatorID},
                {"_creatorCharacterID", creatorCharacterID},
                {"_dictatorial", false},
                {"_startDate", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public static void CrpAlliancesUpdate (this IDatabase Database, string description, string url, int allianceID, int? executorCorpID)
    {
        Database.QueryProcedure (
            "CrpAlliancesUpdate",
            new Dictionary <string, object>
            {
                {"_description", description},
                {"_url", url},
                {"_allianceID", allianceID},
                {"_executorCorpID", executorCorpID}
            }
        );
    }

    public static Rowset CrpAlliancesList (this IDatabase Database)
    {
        return Database.Rowset ("CrpAlliancesList");
    }

    public static Row CrpAlliancesGet (this IDatabase Database, int allianceID)
    {
        return Database.Row ("CrpAlliancesGet", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static IndexRowset CrpAlliancesGetRelationships (this IDatabase Database, int allianceID)
    {
        return Database.IndexRowset (0, "CrpAlliancesGetRelationships", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static Rowset CrpAlliancesGetMembersPublic (this IDatabase Database, int allianceID)
    {
        return Database.Rowset ("CrpAlliancesGetMembersPublic", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static IndexRowset CrpAlliancesGetMembersPrivate (this IDatabase Database, int allianceID)
    {
        return Database.IndexRowset (0, "CrpAlliancesGetMembersPrivate", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static void CrpAlliancesUpdateRelationship (this IDatabase Database, int fromID, int toID, int relationship)
    {
        Database.QueryProcedure (
            "CrpAlliancesUpdateRelationship", new Dictionary <string, object> ()
            {
                {"_fromID", fromID},
                {"_toID", toID},
                {"_relationship", relationship}
            }
        );
    }

    public static void CrpAlliancesRemoveRelationship (this IDatabase Database, int fromID, int toID)
    {
        Database.QueryProcedure (
            "CrpAlliancesRemoveRelationship",
            new Dictionary <string, object>
            {
                {"_fromID", fromID},
                {"_toID", toID}
            }
        );
    }

    public static int? CrpAlliancesUpdateSupportedExecutor (this IDatabase Database, int corporationID, int chosenExecutorID, int allianceID)
    {
        return Database.Scalar <int?> (
            "CrpAlliancesUpdateSupportedExecutor",
            new Dictionary <string, object>
            {
                {"_corporationID", corporationID},
                {"_chosenExecutorID", chosenExecutorID},
                {"_allianceID", allianceID}
            }
        );
    }

    public static IndexRowset CrpAlliancesListApplications (this IDatabase Database, int allianceID)
    {
        return Database.IndexRowset (
            1, "CrpAlliancesListApplications",
            new Dictionary <string, object> {{"_allianceID", allianceID}}
        );
    }

    public static void CrpAlliancesHousekeepApplications (this IDatabase Database, long minimumTime)
    {
        Database.QueryProcedure (
            "CrpAlliancesHousekeepApplications",
            new Dictionary <string, object> {{"_limit", minimumTime}}
        );
    }

    public static void CrpAlliancesUpdateApplication (this IDatabase Database, int corporationID, int allianceID, int newStatus)
    {
        Database.QueryProcedure (
            "CrpAlliancesUpdateApplication",
            new Dictionary <string, object>
            {
                {"_corporationID", corporationID},
                {"_allianceID", allianceID},
                {"_newStatus", newStatus},
                {"_currentTime", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }
}