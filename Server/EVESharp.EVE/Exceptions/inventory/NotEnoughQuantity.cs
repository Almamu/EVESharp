using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.inventory;

public class NotEnoughQuantity : UserError
{
    public NotEnoughQuantity (Type type) : base ("NotEnoughQuantity", new PyDictionary {["typename"] = FormatTypeIDAsName (type.ID)}) { }
}