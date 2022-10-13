using System;
using System.Collections.Generic;
using EVESharp.Database.Extensions;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Types;
using Type = EVESharp.Database.Inventory.Types.Type;

namespace EVESharp.Database.Old;

public class SkillDB : DatabaseAccessor
{
    private ItemDB ItemDB { get; }

    public SkillDB (IDatabase db, ItemDB itemDB) : base (db)
    {
        this.ItemDB = itemDB;
    }

    public int CreateSkill (Type skill, Character character)
    {
        return (int) this.Database.InvCreateItem (
            null, skill, character.ID, character.ID, Flags.Skill,
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