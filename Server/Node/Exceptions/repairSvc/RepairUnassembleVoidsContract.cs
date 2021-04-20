using EVE.Packets.Exceptions;
using Node.Inventory.Items;
using Node.StaticData;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.repairSvc
{
    public class RepairUnassembleVoidsContract : UserError
    {
        public RepairUnassembleVoidsContract(Type type) : base("RepairUnassembleVoidsContract", new PyDictionary {["item"] = type.Name})
        {
        }
    }
}