using System;

namespace EVESharp.Node.Exceptions.Internal
{
    public class SkillMissingException : Exception
    {
        public int SkillTypeID { get; }
        
        public SkillMissingException(StaticData.Inventory.Type skill)
        {
            this.SkillTypeID = skill.ID;
        }

        public SkillMissingException(int skillTypeID)
        {
            this.SkillTypeID = skillTypeID;
        }
    }
}