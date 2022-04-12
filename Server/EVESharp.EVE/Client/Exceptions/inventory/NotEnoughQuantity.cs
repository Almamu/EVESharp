using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.inventory;

public class NotEnoughQuantity : UserError
{
    public NotEnoughQuantity (Type type) : base ("NotEnoughQuantity", new PyDictionary {["typename"] = FormatTypeIDAsName (type.ID)}) { }
}