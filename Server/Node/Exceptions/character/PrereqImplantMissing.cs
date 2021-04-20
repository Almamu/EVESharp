using EVE.Packets.Exceptions;
using Node.Inventory.Items;
using Node.StaticData;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.character
{
    public class PrereqImplantMissing : UserError
    {
        public PrereqImplantMissing(Type requirement) : base("PrereqImplantMissing", new PyDictionary() { ["typeName"] = requirement.Name })
        {
        }
    }
}