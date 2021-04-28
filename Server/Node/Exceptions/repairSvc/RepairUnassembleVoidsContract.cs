using EVE.Packets.Exceptions;
using Node.Inventory.Items;
using Node.StaticData;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.repairSvc
{
    public class RepairUnassembleVoidsContract : UserError
    {
        public RepairUnassembleVoidsContract(int locationID) : base("RepairUnassembleVoidsContract", new PyDictionary {["item"] = FormatLocationID(locationID)})
        {
        }
    }
}