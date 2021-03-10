using System.Collections.Generic;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Skills.Notifications
{
    public class OnSkillStartTraining : PyMultiEventEntry
    {
        public const string NOTIFICATION_NAME = "OnSkillStartTraining";
        
        /// <summary>
        /// The skill this notification is about
        /// </summary>
        public Skill Skill { get; }
        
        public OnSkillStartTraining(Skill skill) : base(NOTIFICATION_NAME)
        {
            this.Skill = skill;
        }

        protected override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.Skill.ID,
                this.Skill.ExpiryTime
            };
        }
    }
}