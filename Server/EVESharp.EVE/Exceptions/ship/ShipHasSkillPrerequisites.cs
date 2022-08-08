using EVESharp.EVE.Data.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Exceptions.ship;

public class ShipHasSkillPrerequisites : UserError
{
    public ShipHasSkillPrerequisites (Type type, PyList <PyInteger> requiredSkills) : base (
        "ShipHasSkillPrerequisites",
        new PyDictionary
        {
            ["itemName"]       = FormatTypeIDAsName (type.ID),
            ["requiredSkills"] = FormatItemTypeList (requiredSkills)
        }
    ) { }
}