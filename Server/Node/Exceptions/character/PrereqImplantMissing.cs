using Node.Inventory.Items;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.character
{
    public class PrereqImplantMissing : UserError
    {
        public PrereqImplantMissing(ItemType requirement) : base("PrereqImplantMissing", new PyDictionary() { ["typeName"] = requirement.Name })
        {
        }
    }
}