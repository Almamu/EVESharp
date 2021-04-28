using EVE.Packets.Exceptions;
using Node.Inventory.Items;
using Node.StaticData;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.reprocessingSvc
{
    public class QuantityLessThanMinimumPortion : UserError
    {
        public QuantityLessThanMinimumPortion(Type type) : base("QuantityLessThanMinimumPortion", new PyDictionary{["typename"] = FormatTypeIDAsName(type.ID), ["portion"] = type.PortionSize})
        {
        }
    }
}