using System.Collections.Generic;
using EVE.Packets.Complex;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Skills
{
    public class OnSkillStartTraining : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnSkillStartTraining";
        
        /// <summary>
        /// The skill this notification is about
        /// </summary>
        public Skill Skill { get; }
        
        public OnSkillStartTraining(Skill skill) : base(NOTIFICATION_NAME)
        {
            this.Skill = skill;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.Skill.ID,
                this.Skill.ExpiryTime
            };
        }
    }
}