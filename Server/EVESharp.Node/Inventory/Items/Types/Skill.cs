using System;
using EVESharp.EVE.Data.Inventory;
using Attribute = EVESharp.EVE.Inventory.Attributes.Attribute;

namespace EVESharp.Node.Inventory.Items.Types;

public class Skill : ItemEntity
{
    private readonly double mSkillPointMultiplier;

    public long Level
    {
        get => Attributes [AttributeTypes.skillLevel].Integer;
        set
        {
            Attributes [AttributeTypes.skillLevel].Integer = value;
            Points                                         = this.GetSkillPointsForLevel (value);
        }
    }

    public double Points
    {
        get => Attributes [AttributeTypes.skillPoints].Float;
        set => Attributes [AttributeTypes.skillPoints].Float = value;
    }

    public Attribute TimeConstant => Attributes [AttributeTypes.skillTimeConstant];

    public Attribute PrimaryAttribute => Attributes [AttributeTypes.primaryAttribute];

    public Attribute SecondaryAttribute => Attributes [AttributeTypes.secondaryAttribute];

    public long ExpiryTime
    {
        get => Attributes [AttributeTypes.expiryTime].Integer;
        set => Attributes [AttributeTypes.expiryTime].Integer = value;
    }

    public Skill (Information.Item info, double skillPointMultiplier) : base (info)
    {
        this.mSkillPointMultiplier = skillPointMultiplier;
    }

    public double GetSkillPointsForLevel (long level)
    {
        if (level > 5 || level == 0)
            return 0;

        return Math.Ceiling (TimeConstant * this.mSkillPointMultiplier * Math.Pow (2, 2.5 * (level - 1)));
    }
}