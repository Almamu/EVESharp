using System;
using EVESharp.Database.Corporations;

namespace EVESharp.EVE.Corporations;

/// <summary>
/// Simple interface that describes how the corporation audit log should be written
/// </summary>
public interface IAudit
{
    /// <summary>
    /// Creates a new audit record with the given data
    /// </summary>
    /// <param name="corporationID"></param>
    /// <param name="eventType"></param>
    /// <param name="characterID"></param>
    /// <param name="dateTime"></param>
    public void RecordAudit(int corporationID, int characterID, DateTime dateTime, CorporationLogEvent eventType);

    /// <summary>
    /// Creates a new audit record with the given data
    /// </summary>
    /// <param name="corporationID"></param>
    /// <param name="characterID"></param>
    /// <param name="eventType"></param>
    public void RecordAudit(int corporationID, int characterID, CorporationLogEvent eventType);

    /// <summary>
    /// Creates a new audit record for roles with the given data
    /// </summary>
    /// <param name="issuerID"></param>
    /// <param name="characterID"></param>
    /// <param name="corporationID"></param>
    /// <param name="oldRoles"></param>
    /// <param name="newRoles"></param>
    /// <param name="grantable"></param>
    public void RecordRoleChange (int issuerID, int characterID, int corporationID, long oldRoles, long newRoles, bool grantable);

    /// <summary>
    /// Creates a new audit record for roles with the given data
    /// </summary>
    /// <param name="issuerID"></param>
    /// <param name="characterID"></param>
    /// <param name="corporationID"></param>
    /// <param name="dateTime"></param>
    /// <param name="oldRoles"></param>
    /// <param name="newRoles"></param>
    /// <param name="grantable"></param>
    public void RecordRoleChange (int issuerID, int characterID, int corporationID, DateTime dateTime, long oldRoles, long newRoles, bool grantable);
}