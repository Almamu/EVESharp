using System.Collections.Generic;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Skills.Notifications
{
    public class OnGodmaMultipleSkillsTrained : PyMultiEventEntry
    {
        private const string NOTIFICATION_NAME = "OnGodmaMultipleSkillsTrained";
        
        public PyList SkillTypeIDs { get; }
        
        public OnGodmaMultipleSkillsTrained(PyList skillTypeIDs) : base(NOTIFICATION_NAME)
        {
            this.SkillTypeIDs = skillTypeIDs;
        }

        protected override List<PyDataType> GetElements()
        {
            return new List<PyDataType>() {this.SkillTypeIDs};
        }
    }
}