using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.reprocessingSvc;

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