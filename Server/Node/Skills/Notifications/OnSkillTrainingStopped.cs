using System.Collections.Generic;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Skills.Notifications
{
    public class OnSkillTrainingStopped : PyMultiEventEntry
    {
        private const string NOTIFICATION_NAME = "OnSkillTrainingStopped";
        
        /// <summary>
        /// The skill this notification is about
        /// </summary>
        public Skill Skill { get; }
        
        public OnSkillTrainingStopped(Skill skill) : base(NOTIFICATION_NAME)
        {
            this.Skill = skill;
        }

        protected override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.Skill.ID,
                0
            };
        }
    }
}