using System;
using System.Collections.Generic;
using Common.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Database;

namespace Node.Database
{
    public enum SkillHistoryReason
    {
        SkillClonePenalty = 34,
        SkillTrainingStarted = 36,
        SkillTrainingComplete = 37,
        SkillTrainingCancelled = 38,
        GMGiveSkill = 39,
        SkillTrainingComplete2 = 53
    };
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

        public void CreateSkillHistoryRecord(ItemType skill, Character character, SkillHistoryReason reason, double skillPoints)
        {
            Database.PrepareQuery(
                "INSERT INTO chrSkillHistory(characterID, skillTypeID, eventID, logDateTime, absolutePoints)VALUES(@characterID, @skillTypeID, @eventID, @logDateTime, @skillPoints)",
                new Dictionary<string, object>()
                {
                    {"@characterID", character.ID},
                    {"@skillTypeID", skill.ID},
                    {"@eventID", (int) reason},
                    {"@logDateTime", DateTime.Now.ToFileTimeUtc()},
                    {"@skillPoints", skillPoints}
                }
            );
        }
        
        public Rowset GetSkillHistory(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT skillTypeID, eventID, logDateTime, absolutePoints FROM chrSkillHistory WHERE characterID=@characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }
    }
}