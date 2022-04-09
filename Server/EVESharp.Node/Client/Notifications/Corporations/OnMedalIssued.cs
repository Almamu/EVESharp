using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Client.Notifications.Corporations;

public class OnMedalIssued : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnMedalIssued";

    public OnMedalIssued () : base (NOTIFICATION_NAME) { }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType> ();
    }
}