using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Inventory.Items;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.character;

public class OnlyOneImplantActive : UserError
{
    public OnlyOneImplantActive(ItemEntity implant) : base("OnlyOneImplantActive", new PyDictionary() { ["typeName"] = FormatTypeIDAsName(implant.Type.ID) })
    {
    }
}