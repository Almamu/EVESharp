using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CrpAccessDenied : UserError
    {
        public CrpAccessDenied(string reason) : base("CrpAccessDenied", new PyDictionary {["reason"] = reason})
        {
        }
    }
}