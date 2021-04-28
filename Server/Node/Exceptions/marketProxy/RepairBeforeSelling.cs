using EVE.Packets.Exceptions;
using Node.Inventory.Items;
using Node.StaticData;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.marketProxy
{
    public class RepairBeforeSelling : UserError
    {
        public RepairBeforeSelling(Type type) : base("RepairBeforeSelling", new PyDictionary {["item"] = FormatTypeIDAsName(type.ID), ["otheritem"] = FormatTypeIDAsName(type.ID)})
        {
        }
    }
}