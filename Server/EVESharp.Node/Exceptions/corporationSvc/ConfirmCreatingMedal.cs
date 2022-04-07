using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.corporationSvc;

public class ConfirmCreatingMedal : UserError
{
    public ConfirmCreatingMedal(int cost) : base("ConfirmCreatingMedal", new PyDictionary {["cost"] = cost})
    {
    }
}