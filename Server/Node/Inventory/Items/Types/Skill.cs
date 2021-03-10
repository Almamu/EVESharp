using System;
using Node.Inventory.Items.Attributes;

namespace Node.Inventory.Items.Types
{
    public class Skill : ItemEntity
    {
        public Skill(ItemEntity from) : base(from)
        {
        }

        public long Level
        {
            get => this.Attributes[AttributeEnum.skillLevel].Integer;
            set
            {
                this.Attributes[AttributeEnum.skillLevel].Integer = value;
                this.Points = this.GetSkillPointsForLevel(value);
            }
        }

        public double Points
        {
            get => this.Attributes[AttributeEnum.skillPoints].Float;
            set => this.Attributes[AttributeEnum.skillPoints].Float = value;
        }

        public ItemAttribute TimeConstant
        {
            get => this.Attributes[AttributeEnum.skillTimeConstant];
        }

        public ItemAttribute PrimaryAttribute
        {
            get => this.Attributes[AttributeEnum.primaryAttribute];
        }

        public ItemAttribute SecondaryAttribute
        {
            get => this.Attributes[AttributeEnum.secondaryAttribute];
        }

        public long ExpiryTime
        {
            get => this.Attributes[AttributeEnum.expiryTime].Integer;
            set => this.Attributes[AttributeEnum.expiryTime].Integer = value;
        }

        public double GetSkillPointsForLevel(long level)
        {
            if (level > 5 || level == 0)
                return 0;

            return Math.Ceiling (TimeConstant * 250 * Math.Pow(2, 2.5 * (level - 1)));
        }
    }
}