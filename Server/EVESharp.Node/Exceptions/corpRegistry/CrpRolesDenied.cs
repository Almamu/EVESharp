using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CrpRolesDenied : UserError
{
    public CrpRolesDenied (int memberID) : base ("CrpRolesDenied", new PyDictionary {["member"] = FormatOwnerID (memberID)}) { }
}