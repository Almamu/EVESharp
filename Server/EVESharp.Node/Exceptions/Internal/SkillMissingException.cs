using System;
using Type = EVESharp.Node.StaticData.Inventory.Type;

namespace EVESharp.Node.Exceptions.Internal;

public class SkillMissingException : Exception
{
    public int SkillTypeID { get; }

    public SkillMissingException (Type skill)
    {
        SkillTypeID = skill.ID;
    }

    public SkillMissingException (int skillTypeID)
    {
        SkillTypeID = skillTypeID;
    }
}