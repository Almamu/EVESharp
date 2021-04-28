using EVE.Packets.Exceptions;
using Node.Inventory.Items;
using Node.StaticData;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.skillMgr
{
    public class CharacterAlreadyKnowsSkill : UserError
    {
        public CharacterAlreadyKnowsSkill(Type skillType) : base("CharacterAlreadyKnowsSkill",
            new PyDictionary {["skillName"] = FormatTypeIDAsName(skillType.ID)})
        {
        }
    }
}