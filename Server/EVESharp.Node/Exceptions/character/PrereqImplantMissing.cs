using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.StaticData;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.character
{
    public class PrereqImplantMissing : UserError
    {
        public PrereqImplantMissing(Type requirement) : base("PrereqImplantMissing", new PyDictionary() { ["typeName"] = FormatTypeIDAsName(requirement.ID) })
        {
        }
    }
}