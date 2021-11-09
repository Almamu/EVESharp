using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.facWarMgr
{
    public class FactionCharJoinDenied : UserError
    {
        public FactionCharJoinDenied(string reason, int hoursLeft) : base("FactionCharJoinDenied", new PyDictionary {["reason"] = reason, ["hours"] = hoursLeft})
        {
        }
    }
}