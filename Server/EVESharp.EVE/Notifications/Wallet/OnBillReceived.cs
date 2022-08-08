using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Notifications.Wallet;

public class OnBillReceived : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnBillReceived";

    public OnBillReceived () : base (NOTIFICATION_NAME) { }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType> ();
    }
}