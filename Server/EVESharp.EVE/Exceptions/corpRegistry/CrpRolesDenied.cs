using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CrpRolesDenied : UserError
{
    public CrpRolesDenied (int memberID) : base ("CrpRolesDenied", new PyDictionary {["member"] = FormatOwnerID (memberID)}) { }
}