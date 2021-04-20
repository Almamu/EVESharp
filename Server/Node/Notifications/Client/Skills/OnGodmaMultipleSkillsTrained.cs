using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Skills
{
    public class OnGodmaMultipleSkillsTrained : ClientNotification
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