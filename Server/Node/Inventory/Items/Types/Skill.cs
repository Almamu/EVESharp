using System;
using Node.Inventory.Items.Attributes;
using Attribute = Node.Inventory.Items.Attributes.Attribute;

namespace Node.Inventory.Items.Types
{
    public class Skill : ItemEntity
    {
        public Skill(ItemEntity from) : base(from)
        {
        }

        public long Level
        {
            get => this.Attributes[StaticData.Inventory.Attributes.skillLevel].Integer;
            set
            {
                this.Attributes[StaticData.Inventory.Attributes.skillLevel].Integer = value;
                this.Points = this.GetSkillPointsForLevel(value);
            }
        }

        public double Points
        {
            get => this.Attributes[StaticData.Inventory.Attributes.skillPoints].Float;
            set => this.Attributes[StaticData.Inventory.Attributes.skillPoints].Float = value;
        }

        public Attribute TimeConstant
        {
            get => this.Attributes[StaticData.Inventory.Attributes.skillTimeConstant];
        }

        public Attribute PrimaryAttribute
        {
            get => this.Attributes[StaticData.Inventory.Attributes.primaryAttribute];
        }

        public Attribute SecondaryAttribute
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

            return Math.Ceiling (TimeConstant * 250 * Math.Pow(2, 2.5 * (level - 1)));
        }
    }
}