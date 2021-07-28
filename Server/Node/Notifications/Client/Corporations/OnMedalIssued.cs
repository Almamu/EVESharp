using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Corporations
{
    public class OnMedalIssued : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnMedalIssued";
        
        public OnMedalIssued() : base(NOTIFICATION_NAME)
        {
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>();
        }
    }
}