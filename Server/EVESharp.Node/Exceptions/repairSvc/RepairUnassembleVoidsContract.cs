using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.StaticData;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.repairSvc
{
    public class RepairUnassembleVoidsContract : UserError
    {
        public RepairUnassembleVoidsContract(int locationID) : base("RepairUnassembleVoidsContract", new PyDictionary {["item"] = FormatLocationID(locationID)})
        {
        }
    }
}