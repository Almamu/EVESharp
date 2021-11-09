using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.StaticData;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.skillMgr
{
    public class CharacterAlreadyKnowsSkill : UserError
    {
        public CharacterAlreadyKnowsSkill(Type skillType) : base("CharacterAlreadyKnowsSkill",
            new PyDictionary {["skillName"] = FormatTypeIDAsName(skillType.ID)})
        {
        }
    }
}