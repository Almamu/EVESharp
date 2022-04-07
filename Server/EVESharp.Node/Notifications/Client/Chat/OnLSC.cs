using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Chat;

public class OnLSC : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnLSC";
        
    public int?       AllianceID      { get; init; }
    public int?       CorporationID   { get; init; }
    public int?       CharacterID     { get; init; }
    public ulong?     Role            { get; init; }
    public long?      CorporationRole { get; init; }
    public int?       WarFactionID    { get; init; }
    public string     Type            { get; init; }
    public PyDataType Channel         { get; init; }
    public PyTuple    Arguments       { get; init; }
        
    public OnLSC(Session session, string type, PyDataType channel, PyTuple args) : base(NOTIFICATION_NAME)
    {
        this.AllianceID      = session.AllianceID;
        this.CorporationID   = session.CorporationID;
        this.CharacterID     = session.CharacterID;
        this.Role            = session.Role;
        this.CorporationRole = session.CorporationRole;
        this.WarFactionID    = session.WarFactionID;
        this.Type            = type;
        this.Channel         = channel;
        this.Arguments       = args;
    }

    public override List<PyDataType> GetElements()
    {
        PyTuple who = new PyTuple(6)
        {
            [0] = this.AllianceID,
            [1] = this.CorporationID,
            [2] = this.CharacterID,
            [3] = this.Role,
            [4] = this.CorporationRole,
            [5] = this.WarFactionID
        };

        return new List<PyDataType>()
        {
            this.Channel,
            1,
            this.Type,
            who,
            this.Arguments
        };
    }
}