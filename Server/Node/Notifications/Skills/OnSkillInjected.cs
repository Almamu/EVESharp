using System.Collections.Generic;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Skills
{
    public class OnSkillInjected : PyNotification
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