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
        private ItemDB ItemDB { get; }

        public SkillDB(DatabaseConnection db, ItemDB itemDB) : base(db)
        {
            this.ItemDB = itemDB;
        }

        public int CreateSkill(ItemType skill, Character character)
        {
            return (int) this.ItemDB.CreateItem(
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
                    {"@logDateTime", DateTime.UtcNow.ToFileTimeUtc()},
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