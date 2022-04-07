using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Station;

public class OnCharNoLongerInStation : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnCharNoLongerInStation";

    public int? CharacterID   { get; init; }
    public int? CorporationID { get; init; }
    public int? AllianceID    { get; init; }
    public int? WarFactionID  { get; init; }

    public OnCharNoLongerInStation (Session session) : base (NOTIFICATION_NAME)
    {
        CharacterID   = session.CharacterID;
        CorporationID = session.CorporationID;
        AllianceID    = session.AllianceID;
        WarFactionID  = session.WarFactionID;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            new PyTuple (4)
            {
                [0] = CharacterID,
                [1] = CorporationID,
                [2] = AllianceID,
                [3] = WarFactionID
            }
        };
    }
}