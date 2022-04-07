using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Database;
using Type = EVESharp.Node.StaticData.Inventory.Type;

namespace EVESharp.Node.Database;

public enum SkillHistoryReason
{
    None                   = 0,
    SkillClonePenalty      = 34,
    SkillTrainingStarted   = 36,
    SkillTrainingComplete  = 37,
    SkillTrainingCancelled = 38,
    GMGiveSkill            = 39,
    SkillTrainingComplete2 = 53
}
    
public class SkillDB : DatabaseAccessor
{
    private ItemDB ItemDB { get; }

    public int CreateSkill(StaticData.Inventory.Type skill, Character character)
    {
        return (int) this.ItemDB.CreateItem(
            null, skill, character, character, Flags.Skill,
            false, true, 1, null, null, null, null
        );
    }

    public void CreateSkillHistoryRecord(StaticData.Inventory.Type skill, Character character, SkillHistoryReason reason, double skillPoints)
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

    public SkillDB(DatabaseConnection db, ItemDB itemDB) : base(db)
    {
        this.ItemDB = itemDB;
    }
}