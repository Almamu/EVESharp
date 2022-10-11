using System;
using EVESharp.Database;
using EVESharp.EVE.Corporations;
using EVESharp.EVE.Data.Corporation;

namespace EVESharp.Node.Corporations;

public class Audit : IAudit
{
    private IDatabaseConnection DB { get; }
    
    public Audit (IDatabaseConnection databaseConnection)
    {
        this.DB = databaseConnection;
    }
    
    public void RecordAudit (int corporationID, int characterID, DateTime dateTime, CorporationLogEvent eventType)
    {
        DB.CrpAuditLogCreate (corporationID, characterID, dateTime, eventType);
    }

    public void RecordAudit (int corporationID, int characterID, CorporationLogEvent eventType)
    {
        this.RecordAudit (corporationID, characterID, DateTime.Now, eventType);
    }

    public void RecordRoleChange (int issuerID, int characterID, int corporationID, long oldRoles, long newRoles, bool grantable)
    {
        this.RecordRoleChange (issuerID, characterID, corporationID, DateTime.Now, oldRoles, newRoles, grantable); 
    }

    public void RecordRoleChange (int issuerID, int characterID, int corporationID, DateTime dateTime, long oldRoles, long newRoles, bool grantable)
    {
        DB.CrpAuditRoleCreate (characterID, issuerID, corporationID, dateTime, grantable, oldRoles, newRoles);
    }
}