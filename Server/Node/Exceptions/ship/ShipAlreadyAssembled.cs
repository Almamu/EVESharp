using EVE.Packets.Exceptions;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.ship
{
    public class ShipAlreadyAssembled : UserError
    {
        public ShipAlreadyAssembled(Type type) : base("ShipAlreadyAssembled", new PyDictionary { ["type"] = FormatTypeIDAsName(type.ID) })
        {
        }
    }
}