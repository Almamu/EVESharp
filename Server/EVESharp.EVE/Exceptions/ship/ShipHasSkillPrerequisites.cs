using EVESharp.EVE.Data.Inventory;
using EVESharp.Types;
using EVESharp.Types.Collections;

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