using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.inventory
{
    public class NotEnoughQuantity : UserError
    {
        public NotEnoughQuantity(Type type) : base("NotEnoughQuantity", new PyDictionary() {["typename"] = type.Name})
        {
        }
    }
}