using Node.Inventory.Items;
using Node.StaticData;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.marketProxy
{
    public class RepairBeforeSelling : UserError
    {
        public RepairBeforeSelling(Type type) : base("RepairBeforeSelling", new PyDictionary {["item"] = type.Name, ["otheritem"] = type.Name})
        {
        }
    }
}