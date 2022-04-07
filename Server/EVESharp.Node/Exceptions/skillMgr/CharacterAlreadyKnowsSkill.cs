using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.skillMgr;

public class CharacterAlreadyKnowsSkill : UserError
{
    public CharacterAlreadyKnowsSkill (Type skillType) : base (
        "CharacterAlreadyKnowsSkill",
        new PyDictionary {["skillName"] = FormatTypeIDAsName (skillType.ID)}
    ) { }
}