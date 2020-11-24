using System;
using Node.Inventory.Items.Attributes;

namespace Node.Inventory.Items.Types
{
    public class Skill : ItemEntity
    {
        public Skill(string entityName, int entityId, ItemType type, ItemEntity entityOwner, ItemEntity entityLocation,
            ItemFlags entityFlag, bool entityContraband, bool entitySingleton, int entityQuantity, double entityX,
            double entityY, double entityZ, string entityCustomInfo, AttributeList attributes, ItemFactory itemFactory)
            : base(entityName, entityId, type, entityOwner, entityLocation, entityFlag, entityContraband,
                entitySingleton, entityQuantity, entityX, entityY, entityZ, entityCustomInfo, attributes, itemFactory)
        {
        }

        public Skill(ItemEntity from) : base(from)
        {
        }

        public int Level
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

        public int TimeConstant
        {
            get => this.Attributes[AttributeEnum.skillTimeConstant].Integer;
            set => this.Attributes[AttributeEnum.skillTimeConstant].Integer = value;
        }

        public double GetSkillPointsForLevel(int level)
        {
            if (level >= 5)
                return 0;
            if (level == 0)
                return TimeConstant * 250;

            return (int) ((TimeConstant * 250) * Math.Pow(2, 2.5 * level));
        } 
    }
}