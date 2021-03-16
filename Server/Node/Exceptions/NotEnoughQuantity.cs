using Node.Inventory.Items;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class NotEnoughQuantity : UserError
    {
        public NotEnoughQuantity(ItemType type) : base("NotEnoughQuantity", new PyDictionary() {["typename"] = type.Name})
        {
        }
    }
}