using EVE.Packets.Exceptions;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.ship
{
    public class ShipHasSkillPrerequisites : UserError
    {
        public ShipHasSkillPrerequisites(Type type, PyList<PyInteger> requiredSkills) : base("ShipHasSkillPrerequisites",
            new PyDictionary {["itemName"] = FormatTypeIDAsName(type.ID), ["requiredSkills"] = FormatItemTypeList(requiredSkills)})
        {
        }
    }
}