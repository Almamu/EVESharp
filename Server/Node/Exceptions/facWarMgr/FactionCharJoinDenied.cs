using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.facWarMgr
{
    public class FactionCharJoinDenied : UserError
    {
        public FactionCharJoinDenied(string reason, int hoursLeft) : base("FactionCharJoinDenied", new PyDictionary {["reason"] = reason, ["hours"] = hoursLeft})
        {
        }
    }
}