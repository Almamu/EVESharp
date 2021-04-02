using Node.Inventory.Items;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.reprocessingSvc
{
    public class QuantityLessThanMinimumPortion : UserError
    {
        public QuantityLessThanMinimumPortion(ItemType type) : base("QuantityLessThanMinimumPortion", new PyDictionary{["typename"] = type.Name, ["portion"] = type.PortionSize})
        {
        }
    }
}