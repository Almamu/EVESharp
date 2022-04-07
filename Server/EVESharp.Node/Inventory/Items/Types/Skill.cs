using System;
using EVESharp.Node.Inventory.Items.Attributes;
using Attribute = EVESharp.Node.Inventory.Items.Attributes.Attribute;

namespace EVESharp.Node.Inventory.Items.Types;

public class Skill : ItemEntity
{
    private readonly double mSkillPointMultiplier;
        
    public Skill(Information.Item info, double skillPointMultiplier) : base(info)
    {
        this.mSkillPointMultiplier = skillPointMultiplier;
    }

    public long Level
    {
        get => this.Attributes[StaticData.Inventory.Attributes.skillLevel].Integer;
        set
        {
            this.Attributes[StaticData.Inventory.Attributes.skillLevel].Integer = value;
            this.Points                                                         = this.GetSkillPointsForLevel(value);
        }
    }

    public double Points
    {
        get => this.Attributes[StaticData.Inventory.Attributes.skillPoints].Float;
        set => this.Attributes[StaticData.Inventory.Attributes.skillPoints].Float = value;
    }

    public Attributes.Attribute TimeConstant
    {
        get => this.Attributes[StaticData.Inventory.Attributes.skillTimeConstant];
    }

    public Attributes.Attribute PrimaryAttribute
    {
        get => this.Attributes[StaticData.Inventory.Attributes.primaryAttribute];
    }

    public Attributes.Attribute SecondaryAttribute
    {
        get => this.Attributes[StaticData.Inventory.Attributes.secondaryAttribute];
    }

    public long ExpiryTime
    {
        get => this.Attributes[StaticData.Inventory.Attributes.expiryTime].Integer;
        set => this.Attributes[StaticData.Inventory.Attributes.expiryTime].Integer = value;
    }

    public double GetSkillPointsForLevel(long level)
    {
        if (level > 5 || level == 0)
            return 0;

        return Math.Ceiling (TimeConstant * this.mSkillPointMultiplier * Math.Pow(2, 2.5 * (level - 1)));
    }
}