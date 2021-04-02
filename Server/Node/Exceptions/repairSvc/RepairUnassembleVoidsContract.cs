using Node.Inventory.Items;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.repairSvc
{
    public class RepairUnassembleVoidsContract : UserError
    {
        public RepairUnassembleVoidsContract(ItemType type) : base("RepairUnassembleVoidsContract", new PyDictionary {["item"] = type.Name})
        {
        }
    }
}