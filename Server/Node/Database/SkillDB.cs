using Common.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;

namespace Node.Database
{
    public class SkillDB : DatabaseAccessor
    {
        private readonly ItemFactory mItemFactory;
        private readonly ItemDB mItemDB;

        public SkillDB(DatabaseConnection db, ItemFactory itemFactory) : base(db)
        {
            this.mItemFactory = itemFactory;
            this.mItemDB = new ItemDB(db, this.mItemFactory);
        }

        public int CreateSkill(ItemType skill, Character character)
        {
            return (int) this.mItemDB.CreateItem(
                skill.Name, skill, character, character, ItemFlags.Skill,
                false, true, 1, 0, 0, 0, null
            );
        }
    }
}