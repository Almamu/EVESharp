using System.Collections.Generic;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Skills.Notifications
{
    public class OnGodmaMultipleSkillsTrained : PyMultiEventEntry
    {
        private const string NOTIFICATION_NAME = "OnGodmaMultipleSkillsTrained";
        
        public PyList<PyInteger> SkillTypeIDs { get; }
        
        public OnGodmaMultipleSkillsTrained(PyList<PyInteger> skillTypeIDs) : base(NOTIFICATION_NAME)
        {
            this.SkillTypeIDs = skillTypeIDs;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>() {this.SkillTypeIDs};
        }
    }
}