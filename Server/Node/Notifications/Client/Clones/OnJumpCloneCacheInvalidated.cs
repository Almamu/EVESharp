using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Clones
{
    public class OnJumpCloneCacheInvalidated : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnJumpCloneCacheInvalidated";
        
        public OnJumpCloneCacheInvalidated() : base(NOTIFICATION_NAME)
        {
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>();
        }
    }
}