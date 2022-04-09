using System;
using Type = EVESharp.EVE.StaticData.Inventory.Type;

namespace EVESharp.EVE.Client.Exceptions.Internal;

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