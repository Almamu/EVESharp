using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.insuranceSvc
{
    public class InsureShipFailed : UserError
    {
        public InsureShipFailed(string reason) : base("InsureShipFailed",
            new PyDictionary {["reason"] = reason})
        {
        }
    }
}