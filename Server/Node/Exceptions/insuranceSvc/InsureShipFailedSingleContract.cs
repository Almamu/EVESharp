using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.insuranceSvc
{
    public class InsureShipFailedSingleContract : UserError
    {
        public InsureShipFailedSingleContract(string ownerName) : base("InsureShipFailedSingleContract",
            new PyDictionary {["ownerName"] = ownerName})
        {
        }
    }
}