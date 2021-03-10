using System.Collections.Generic;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Skills.Notifications
{
    public class OnSkillInjected : PyMultiEventEntry
    {
        private const string NOTIFICATION_NAME = "OnSkillInjected";
        
        public OnSkillInjected() : base(NOTIFICATION_NAME)
        {
        }

        protected override List<PyDataType> GetElements()
        {
            return null;
        }
    }
}