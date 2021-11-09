using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.insuranceSvc
{
    public class InsureShipFailedSingleContract : UserError
    {
        public InsureShipFailedSingleContract(int ownerID) : base("InsureShipFailedSingleContract",
            new PyDictionary {["ownerName"] = FormatOwnerID(ownerID)})
        {
        }
    }
}