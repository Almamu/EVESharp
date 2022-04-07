using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Character;

public class OnStandingSet : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnStandingSet";

    public int    FromID      { get; init; }
    public int    ToID        { get; init; }
    public double NewStanding { get; init; }

    public OnStandingSet (int fromID, int toID, double newStanding) : base (NOTIFICATION_NAME)
    {
        FromID      = fromID;
        ToID        = toID;
        NewStanding = newStanding;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            FromID,
            ToID,
            NewStanding
        };
    }
}