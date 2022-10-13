using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.skillMgr;

public class CharacterAlreadyKnowsSkill : UserError
{
    public CharacterAlreadyKnowsSkill (Type skillType) : base (
        "CharacterAlreadyKnowsSkill",
        new PyDictionary {["skillName"] = FormatTypeIDAsName (skillType.ID)}
    ) { }
}