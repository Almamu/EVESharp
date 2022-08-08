using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Notifications.Clones;

public class OnJumpCloneCacheInvalidated : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnJumpCloneCacheInvalidated";

    public OnJumpCloneCacheInvalidated () : base (NOTIFICATION_NAME) { }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType> ();
    }
}