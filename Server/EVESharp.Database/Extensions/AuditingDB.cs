using System;
using System.Collections.Generic;
using EVESharp.Database.Corporations;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Extensions;

public static class AuditingDB
{
    public static void CrpAuditLogCreate (this IDatabase Database, int corporationID, int characterID, DateTime eventDatetime, CorporationLogEvent eventTypeID)
    {
        Database.QueryProcedure (
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

    public static PyList<PyPackedRow> CrpGetAuditLog (this IDatabase Database, int corporationID, int characterID, long fromDate, long toDate, int rowsPerPage = 10)
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
        this IDatabase Database, int characterID, int issuerID, int corporationID, DateTime changeTime, bool grantable, long oldRoles,
        long                     newRoles
    )
    {
        Database.QueryProcedure (
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

    public static PyList<PyPackedRow> CrpGetAuditRole (this IDatabase Database, int corporationID, int characterID, long fromDate, long toDate, int rowsPerPage = 10)
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