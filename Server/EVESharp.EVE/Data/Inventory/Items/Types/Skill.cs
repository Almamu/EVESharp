using EVESharp.Database.EVEMath;
using EVESharp.Database.Inventory.Attributes;
using Attribute = EVESharp.Database.Inventory.Attributes.Attribute;

namespace EVESharp.EVE.Data.Inventory.Items.Types;

public class Skill : ItemEntity
{
    private readonly double mSkillPointMultiplier;

    public long Level
    {
        get => this.Attributes [AttributeTypes.skillLevel].Integer;
        set
        {
            this.Attributes [AttributeTypes.skillLevel].Integer = value;
            this.Points                                         = this.GetSkillPointsForLevel (value);
        }
    }

    public double Points
    {
        get => this.Attributes [AttributeTypes.skillPoints].Float;
        set => this.Attributes [AttributeTypes.skillPoints].Float = value;
    }

    public Attribute TimeConstant => this.Attributes [AttributeTypes.skillTimeConstant];

    public Attribute PrimaryAttribute => this.Attributes [AttributeTypes.primaryAttribute];

    public Attribute SecondaryAttribute => this.Attributes [AttributeTypes.secondaryAttribute];

    public long ExpiryTime
    {
        get => this.Attributes [AttributeTypes.expiryTime].Integer;
        set => this.Attributes [AttributeTypes.expiryTime].Integer = value;
    }

    public Skill (Database.Inventory.Types.Information.Item info, double skillPointMultiplier) : base (info)
    {
        this.mSkillPointMultiplier = skillPointMultiplier;
    }

    public double GetSkillPointsForLevel (long level)
    {
        return Skills.GetSkillPointsForLevel (level, this.TimeConstant, this.mSkillPointMultiplier);
    }
}