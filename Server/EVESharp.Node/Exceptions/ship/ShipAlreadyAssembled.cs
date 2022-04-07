using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.ship;

public class ShipAlreadyAssembled : UserError
{
    public ShipAlreadyAssembled(Type type) : base("ShipAlreadyAssembled", new PyDictionary { ["type"] = FormatTypeIDAsName(type.ID) })
    {
    }
}