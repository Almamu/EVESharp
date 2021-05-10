using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpStationMgr
{
    public class RentingAnOfficeCostsMore : UserError
    {
        public RentingAnOfficeCostsMore(int amount) : base("RentingAnOfficeCostsMore", new PyDictionary {["amount"] = FormatISK(amount)})
        {
        }
    }
}