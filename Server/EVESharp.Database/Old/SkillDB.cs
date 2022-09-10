using System;
using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Types;
using Type = EVESharp.EVE.Data.Inventory.Type;

namespace EVESharp.Database.Old;

public class SkillDB : DatabaseAccessor
{
    private ItemDB ItemDB { get; }

    public SkillDB (IDatabaseConnection db, ItemDB itemDB) : base (db)
    {
        this.ItemDB = itemDB;
    }

    public int CreateSkill (Type skill, Character character)
    {
        return (int) this.Database.InvCreateItem (
            null, skill, character, character, Flags.Skill,
            false, true, 1, null, null, null, null
        );
    }

    public void CreateSkillHistoryRecord (Type skill, Character character, SkillHistoryReason reason, double skillPoints)
    {
        this.Database.Prepare (
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
        return this.Database.PrepareRowset (
            "SELECT skillTypeID, eventID, logDateTime, absolutePoints FROM chrSkillHistory WHERE characterID=@characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }
}