using Node.Inventory.Items;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.skillMgr
{
    public class CharacterAlreadyKnowsSkill : UserError
    {
        public CharacterAlreadyKnowsSkill(ItemType skillType) : base("CharacterAlreadyKnowsSkill",
            new PyDictionary {["skillName"] = skillType.Name})
        {
        }
    }
}