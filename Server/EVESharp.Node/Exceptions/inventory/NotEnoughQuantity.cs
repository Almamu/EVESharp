using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.inventory;

public class NotEnoughQuantity : UserError
{
    public NotEnoughQuantity (Type type) : base ("NotEnoughQuantity", new PyDictionary {["typename"] = FormatTypeIDAsName (type.ID)}) { }
}