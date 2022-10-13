using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.reprocessingSvc;

public class QuantityLessThanMinimumPortion : UserError
{
    public QuantityLessThanMinimumPortion (Type type) : base (
        "QuantityLessThanMinimumPortion", new PyDictionary
        {
            ["typename"] = FormatTypeIDAsName (type.ID),
            ["portion"]  = type.PortionSize
        }
    ) { }
}