using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Database;

public static class AlliancesDB
{
    public static void CrpAlliancesCreate (this DatabaseConnection Database, int allianceID, string shortName, string description, string url, int creatorID, int creatorCharacterID)
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

    public static void CrpAlliancesUpdate (this DatabaseConnection Database, string description, string url, int allianceID, int? executorCorpID)
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

    public static Rowset CrpAlliancesList (this DatabaseConnection Database)
    {
        return Database.Rowset ("CrpAlliancesList");
    }

    public static Row CrpAlliancesGet (this DatabaseConnection Database, int allianceID)
    {
        return Database.Row ("CrpAlliancesGet", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static IndexRowset CrpAlliancesGetRelationships (this DatabaseConnection Database, int allianceID)
    {
        return Database.IndexRowset (0, "CrpAlliancesGetRelationships", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static Rowset CrpAlliancesGetMembersPublic (this DatabaseConnection Database, int allianceID)
    {
        return Database.Rowset ("CrpAlliancesGetMembersPublic", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static IndexRowset CrpAlliancesGetMembersPrivate (this DatabaseConnection Database, int allianceID)
    {
        return Database.IndexRowset (0, "CrpAlliancesGetMembersPrivate", new Dictionary <string, object> () {{"_allianceID", allianceID}});
    }

    public static void CrpAlliancesUpdateRelationship (this DatabaseConnection Database, int fromID, int toID, int relationship)
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

    public static void CrpAlliancesRemoveRelationship (this DatabaseConnection Database, int fromID, int toID)
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

    public static int? CrpAlliancesUpdateSupportedExecutor (this DatabaseConnection Database, int corporationID, int chosenExecutorID, int allianceID)
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

    public static IndexRowset CrpAlliancesListApplications (this DatabaseConnection Database, int allianceID)
    {
        return Database.IndexRowset (
            1, "CrpAlliancesListApplications",
            new Dictionary <string, object> {{"_allianceID", allianceID}}
        );
    }

    public static void CrpAlliancesHousekeepApplications (this DatabaseConnection Database, long minimumTime)
    {
        Database.Procedure (
            "CrpAlliancesHousekeepApplications",
            new Dictionary <string, object> {{"_limit", minimumTime}}
        );
    }
}