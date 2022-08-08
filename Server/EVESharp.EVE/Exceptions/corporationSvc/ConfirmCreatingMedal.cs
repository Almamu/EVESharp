using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.corporationSvc;

public class ConfirmCreatingMedal : UserError
{
    public ConfirmCreatingMedal (int cost) : base ("ConfirmCreatingMedal", new PyDictionary {["cost"] = cost}) { }
}