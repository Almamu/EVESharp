using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.repairSvc;

public class RepairUnassembleVoidsContract : UserError
{
    public RepairUnassembleVoidsContract (int locationID) : base (
        "RepairUnassembleVoidsContract", new PyDictionary {["item"] = FormatLocationID (locationID)}
    ) { }
}