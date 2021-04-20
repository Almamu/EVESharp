using EVE.Packets.Exceptions;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.inventory
{
    public class NotEnoughQuantity : UserError
    {
        public NotEnoughQuantity(Type type) : base("NotEnoughQuantity", new PyDictionary() {["typename"] = type.Name})
        {
        }
    }
}