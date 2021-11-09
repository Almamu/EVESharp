using System;

namespace EVESharp.Node.Exceptions.Internal
{
    public class SkillMissingException : Exception
    {
        public StaticData.Inventory.Type Skill { get; }
        
        public SkillMissingException(StaticData.Inventory.Type skill)
        {
            this.Skill = skill;
        }
    }
}