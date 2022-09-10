using System;
using System.Collections.Generic;
using EVESharp.EVE.Types;

namespace EVESharp.Database;

public static class AlliancesDB
{
    public static void CrpAlliancesCreate (this IDatabaseConnection Database, int allianceID, string shortName, string description, string url, int creatorID, int creatorCharacterID)
    {
        Database.Procedure (
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

    public static void CrpAlliancesUpdate (this IDatabaseConnection Database, string description, string url, int allianceID, int? executorCorpID)
    {
        Database.Procedure (
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

    public static Rowset CrpAlliancesList (this IDatabaseConnection Database)
    {
        return Database.Rowset ("CrpAlliancesList");
    }

    public static Row CrpAlliancesGet (this IDatabaseConnection Database, int allianceID)
    {
        return Database.Row ("CrpAlliancesGet", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static IndexRowset CrpAlliancesGetRelationships (this IDatabaseConnection Database, int allianceID)
    {
        return Database.IndexRowset (0, "CrpAlliancesGetRelationships", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static Rowset CrpAlliancesGetMembersPublic (this IDatabaseConnection Database, int allianceID)
    {
        return Database.Rowset ("CrpAlliancesGetMembersPublic", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static IndexRowset CrpAlliancesGetMembersPrivate (this IDatabaseConnection Database, int allianceID)
    {
        return Database.IndexRowset (0, "CrpAlliancesGetMembersPrivate", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static void CrpAlliancesUpdateRelationship (this IDatabaseConnection Database, int fromID, int toID, int relationship)
    {
        Database.Procedure (
            "CrpAlliancesUpdateRelationship", new Dictionary <string, object> ()
            {
                {"_fromID", fromID},
                {"_toID", toID},
                {"_relationship", relationship}
            }
        );
    }

    public static void CrpAlliancesRemoveRelationship (this IDatabaseConnection Database, int fromID, int toID)
    {
        Database.Procedure (
            "CrpAlliancesRemoveRelationship",
            new Dictionary <string, object>
            {
                {"_fromID", fromID},
                {"_toID", toID}
            }
        );
    }

    public static int? CrpAlliancesUpdateSupportedExecutor (this IDatabaseConnection Database, int corporationID, int chosenExecutorID, int allianceID)
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

    public static IndexRowset CrpAlliancesListApplications (this IDatabaseConnection Database, int allianceID)
    {
        return Database.IndexRowset (
            1, "CrpAlliancesListApplications",
            new Dictionary <string, object> {{"_allianceID", allianceID}}
        );
    }

    public static void CrpAlliancesHousekeepApplications (this IDatabaseConnection Database, long minimumTime)
    {
        Database.Procedure (
            "CrpAlliancesHousekeepApplications",
            new Dictionary <string, object> {{"_limit", minimumTime}}
        );
    }

    public static void CrpAlliancesUpdateApplication (this IDatabaseConnection Database, int corporationID, int allianceID, int newStatus)
    {
        Database.Procedure (
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