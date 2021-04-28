using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.insuranceSvc
{
    public class InsureShipFailedSingleContract : UserError
    {
        public InsureShipFailedSingleContract(int ownerID) : base("InsureShipFailedSingleContract",
            new PyDictionary {["ownerName"] = FormatOwnerID(ownerID)})
        {
        }
    }
}