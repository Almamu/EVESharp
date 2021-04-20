using System.Collections.Generic;
using EVE.Packets.Complex;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Skills
{
    public class OnSkillTrainingStopped : ClientNotification
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

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.Skill.ID,
                0
            };
        }
    }
}