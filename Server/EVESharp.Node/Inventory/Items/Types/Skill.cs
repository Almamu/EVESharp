using System;
using Attribute = EVESharp.EVE.Inventory.Attributes.Attribute;

namespace EVESharp.Node.Inventory.Items.Types;

public class Skill : ItemEntity
{
    private readonly double mSkillPointMultiplier;

    public long Level
    {
        get => Attributes [EVE.StaticData.Inventory.Attributes.skillLevel].Integer;
        set
        {
            Attributes [EVE.StaticData.Inventory.Attributes.skillLevel].Integer = value;
            Points                                                          = this.GetSkillPointsForLevel (value);
        }
    }

    public double Points
    {
        get => Attributes [EVE.StaticData.Inventory.Attributes.skillPoints].Float;
        set => Attributes [EVE.StaticData.Inventory.Attributes.skillPoints].Float = value;
    }

    public Attribute TimeConstant => Attributes [EVE.StaticData.Inventory.Attributes.skillTimeConstant];

    public Attribute PrimaryAttribute => Attributes [EVE.StaticData.Inventory.Attributes.primaryAttribute];

    public Attribute SecondaryAttribute => Attributes [EVE.StaticData.Inventory.Attributes.secondaryAttribute];

    public long ExpiryTime
    {
        get => Attributes [EVE.StaticData.Inventory.Attributes.expiryTime].Integer;
        set => Attributes [EVE.StaticData.Inventory.Attributes.expiryTime].Integer = value;
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