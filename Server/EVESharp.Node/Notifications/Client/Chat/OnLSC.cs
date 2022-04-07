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

    public OnLSC (Session session, string type, PyDataType channel, PyTuple args) : base (NOTIFICATION_NAME)
    {
        AllianceID      = session.AllianceID;
        CorporationID   = session.CorporationID;
        CharacterID     = session.CharacterID;
        Role            = session.Role;
        CorporationRole = session.CorporationRole;
        WarFactionID    = session.WarFactionID;
        Type            = type;
        Channel         = channel;
        Arguments       = args;
    }

    public override List <PyDataType> GetElements ()
    {
        PyTuple who = new PyTuple (6)
        {
            [0] = AllianceID,
            [1] = CorporationID,
            [2] = CharacterID,
            [3] = Role,
            [4] = CorporationRole,
            [5] = WarFactionID
        };

        return new List <PyDataType>
        {
            Channel,
            1,
            Type,
            who,
            Arguments
        };
    }
}