using System;

namespace Node.Exceptions.Internal
{
    public class SkillMissingException : Exception
    {
        public string SkillName { get; }
        
        public SkillMissingException(string skillName)
        {
            this.SkillName = skillName;
        }
    }
}