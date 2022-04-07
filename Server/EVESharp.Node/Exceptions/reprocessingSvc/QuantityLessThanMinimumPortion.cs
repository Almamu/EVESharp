using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.StaticData;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.reprocessingSvc;

public class QuantityLessThanMinimumPortion : UserError
{
    public QuantityLessThanMinimumPortion(Type type) : base("QuantityLessThanMinimumPortion", new PyDictionary{["typename"] = FormatTypeIDAsName(type.ID), ["portion"] = type.PortionSize})
    {
    }
}