using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Skills
{
    public class OnSkillInjected : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnSkillInjected";
        
        public OnSkillInjected() : base(NOTIFICATION_NAME)
        {
        }

        public override List<PyDataType> GetElements()
        {
            return null;
        }
    }
}