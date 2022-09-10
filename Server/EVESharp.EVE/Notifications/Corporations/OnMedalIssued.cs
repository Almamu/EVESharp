using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;

namespace EVESharp.EVE.Notifications.Corporations;

public class OnMedalIssued : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnMedalIssued";

    public OnMedalIssued () : base (NOTIFICATION_NAME) { }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType> ();
    }
}