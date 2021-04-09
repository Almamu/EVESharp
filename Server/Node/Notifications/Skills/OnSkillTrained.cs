using System.Collections.Generic;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Skills
{
    public class OnSkillTrained : PyNotification
    {
        private const string NOTIFICATION_NAME = "OnSkillTrained";
        
        /// <summary>
        /// The skill this notification is about
        /// </summary>
        public Skill Skill { get; }
        
        public OnSkillTrained(Skill skill) : base(NOTIFICATION_NAME)
        {
            this.Skill = skill;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.Skill.ID
            };
        }
    }
}