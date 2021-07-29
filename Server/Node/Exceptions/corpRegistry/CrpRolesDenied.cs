using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CrpRolesDenied : UserError
    {
        public CrpRolesDenied(int memberID) : base("CrpRolesDenied", new PyDictionary {["member"] = FormatOwnerID(memberID)})
        {
        }
    }
}