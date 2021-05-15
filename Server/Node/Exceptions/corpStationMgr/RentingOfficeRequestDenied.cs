using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpStationMgr
{
    public class RentingOfficeRequestDenied : UserError
    {
        public RentingOfficeRequestDenied(string reason) : base("RentingOfficeRequestDenied", new PyDictionary {["reason"] = reason})
        {
        }
    }
}