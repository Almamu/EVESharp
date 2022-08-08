using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.EVE.Data.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.PythonTypes.Types.Database;
using Type = EVESharp.EVE.Data.Inventory.Type;

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

    public SkillDB (IDatabaseConnection db, ItemDB itemDB) : base (db)
    {
        ItemDB = itemDB;
    }

    public int CreateSkill (Type skill, Character character)
    {
        return (int) ItemDB.CreateItem (
            null, skill, character, character, Flags.Skill,
            false, true, 1, null, null, null, null
        );
    }

    public void CreateSkillHistoryRecord (Type skill, Character character, SkillHistoryReason reason, double skillPoints)
    {
        Database.Prepare (
            "INSERT INTO chrSkillHistory(characterID, skillTypeID, eventID, logDateTime, absolutePoints)VALUES(@characterID, @skillTypeID, @eventID, @logDateTime, @skillPoints)",
            new Dictionary <string, object>
            {
                {"@characterID", character.ID},
                {"@skillTypeID", skill.ID},
                {"@eventID", (int) reason},
                {"@logDateTime", DateTime.UtcNow.ToFileTimeUtc ()},
                {"@skillPoints", skillPoints}
            }
        );
    }

    public Rowset GetSkillHistory (int characterID)
    {
        return Database.PrepareRowset (
            "SELECT skillTypeID, eventID, logDateTime, absolutePoints FROM chrSkillHistory WHERE characterID=@characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }
}