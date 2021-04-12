using System.Collections.Generic;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Clones
{
    public class OnJumpCloneCacheInvalidated : PyNotification
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