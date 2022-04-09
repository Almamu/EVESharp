using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.skillMgr;

public class CharacterAlreadyKnowsSkill : UserError
{
    public CharacterAlreadyKnowsSkill (Type skillType) : base (
        "CharacterAlreadyKnowsSkill",
        new PyDictionary {["skillName"] = FormatTypeIDAsName (skillType.ID)}
    ) { }
}