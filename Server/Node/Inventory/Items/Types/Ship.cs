using System;
using System.Collections.Generic;
using System.Linq;
using Node.Exceptions.Internal;
using Node.Exceptions.ship;
using Node.Inventory.Items.Attributes;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items.Types
{
    public class Ship : ItemInventory
    {
        public Ship(ItemEntity from) : base(from)
        {
        }

        protected override void LoadContents(ItemFlags ignoreFlags = ItemFlags.None)
        {
            base.LoadContents(ItemFlags.Pilot);
        }

        private void CheckSkillRequirement(AttributeEnum skillTypeIDRequirement, AttributeEnum skillLevelRequirement, Dictionary<int, Skill> skills)
        {
            if (this.Attributes.AttributeExists(skillLevelRequirement) == false ||
                this.Attributes.AttributeExists(skillTypeIDRequirement) == false)
                return;

            int skillTypeID = (int) this.Attributes[skillTypeIDRequirement];
            int skillLevel = (int) this.Attributes[skillLevelRequirement];

            if (skills.ContainsKey(skillTypeID) == false)
                throw new SkillMissingException(this.mItemFactory.TypeManager[skillTypeID].Name);

            if (skills[skillTypeID].Level < skillLevel)
                throw new SkillMissingException(this.mItemFactory.TypeManager[skillTypeID].Name);
        }

        public void CheckShipPrerequisites(Character character)
        {
            Dictionary<int, Skill> skills = character.InjectedSkillsByTypeID;
            List<string> missingSkills = new List<string>();
            AttributeEnum[] attributes = new AttributeEnum[]
            {
                AttributeEnum.requiredSkill1,
                AttributeEnum.requiredSkill2,
                AttributeEnum.requiredSkill3,
                AttributeEnum.requiredSkill4,
                AttributeEnum.requiredSkill5,
                AttributeEnum.requiredSkill6,
            };
            AttributeEnum[] levelAttributes = new AttributeEnum[]
            {
                AttributeEnum.requiredSkill1Level,
                AttributeEnum.requiredSkill2Level,
                AttributeEnum.requiredSkill3Level,
                AttributeEnum.requiredSkill4Level,
                AttributeEnum.requiredSkill5Level,
                AttributeEnum.requiredSkill6Level,
            };

            for (int i = 0; i < attributes.Length; i++)
            {
                try
                {
                    this.CheckSkillRequirement(attributes[i], levelAttributes[i], skills);
                }
                catch (SkillMissingException e)
                {
                    missingSkills.Add(e.SkillName);
                }
            }

            if (missingSkills.Count > 0)
                throw new ShipHasSkillPrerequisites(this.Type.Name, String.Join(", ", missingSkills));
        }

        public override void Destroy()
        {
            base.Destroy();
            
            // remove insurance off the database
            this.mItemFactory.InsuranceDB.UnInsureShip(this.ID);
        }
    }
}