using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.ship
{
    public class ShipHasSkillPrerequisites : UserError
    {
        public ShipHasSkillPrerequisites(string itemName, string skillNames) : base("ShipHasSkillPrerequisites",
            new PyDictionary {["itemName"] = itemName, ["requiredSkills"] = skillNames})
        {
        }
    }
}