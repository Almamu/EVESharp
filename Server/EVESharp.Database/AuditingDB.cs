using System;
using System.Collections.Generic;
using EVESharp.EVE.Data.Corporation;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database;

public static class AuditingDB
{
    public static void CrpAuditLogCreate (this IDatabaseConnection Database, int corporationID, int characterID, DateTime eventDatetime, CorporationLogEvent eventTypeID)
    {
        Database.Procedure (
            "CrpAuditLogCreate",
            new Dictionary <string, object> ()
            {
                {"_corporationID", corporationID},
                {"_charID", characterID},
                {"_eventDateTime", eventDatetime.ToFileTimeUtc ()},
                {"_eventTypeID", (int) eventTypeID}
            }
        );
    }

    public static PyList<PyPackedRow> CrpGetAuditLog (this IDatabaseConnection Database, int corporationID, int characterID, long fromDate, long toDate, int rowsPerPage = 10)
    {
        return Database.PackedRowList (
            "CrpGetAuditLog",
            new Dictionary <string, object> ()
            {
                {"_corporationID", corporationID},
                {"_charID", characterID},
                {"_fromDate", fromDate},
                {"_toDate", toDate},
                {"_limit", rowsPerPage}
            }
        );
    }

    public static void CrpAuditRoleCreate
    (
        this IDatabaseConnection Database, int characterID, int issuerID, int corporationID, DateTime changeTime, bool grantable, long oldRoles,
        long                     newRoles
    )
    {
        Database.Procedure (
            "CrpAuditRoleCreate",
            new Dictionary <string, object> ()
            {
                {"_charID", characterID},
                {"_issuerID", issuerID},
                {"_corporationID", corporationID},
                {"_changeTime", changeTime.ToFileTimeUtc ()},
                {"_grantable", grantable},
                {"_oldRoles", oldRoles},
                {"_newRoles", newRoles}
            }
        );
    }

    public static PyList<PyPackedRow> CrpGetAuditRole (this IDatabaseConnection Database, int corporationID, int characterID, long fromDate, long toDate, int rowsPerPage = 10)
    {
        return Database.PackedRowList (
            "CrpGetAuditRole",
            new Dictionary <string, object> ()
            {
                {"_corporationID", corporationID},
                {"_charID", characterID},
                {"_fromDate", fromDate},
                {"_toDate", toDate},
                {"_limit", rowsPerPage}
            }
        );
    }
}