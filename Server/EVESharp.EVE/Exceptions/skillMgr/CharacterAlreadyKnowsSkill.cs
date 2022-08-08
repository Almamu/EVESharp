using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.skillMgr;

public class CharacterAlreadyKnowsSkill : UserError
{
    public CharacterAlreadyKnowsSkill (Type skillType) : base (
        "CharacterAlreadyKnowsSkill",
        new PyDictionary {["skillName"] = FormatTypeIDAsName (skillType.ID)}
    ) { }
}