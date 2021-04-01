using Node.Inventory.Items;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.marketProxy
{
    public class RepairBeforeSelling : UserError
    {
        public RepairBeforeSelling(ItemType type) : base("RepairBeforeSelling", new PyDictionary {["item"] = type.Name, ["otheritem"] = type.Name})
        {
        }
    }
}