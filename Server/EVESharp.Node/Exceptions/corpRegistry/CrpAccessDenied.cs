using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.corpRegistry
{
    public class CrpAccessDenied : UserError
    {
        public CrpAccessDenied(string reason) : base("CrpAccessDenied", new PyDictionary {["reason"] = reason})
        {
        }
    }
}