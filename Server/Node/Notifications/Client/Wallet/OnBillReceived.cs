using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Wallet
{
    public class OnBillReceived : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnBillReceived";
        
        public OnBillReceived() : base(NOTIFICATION_NAME)
        {
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>();
        }
    }
}