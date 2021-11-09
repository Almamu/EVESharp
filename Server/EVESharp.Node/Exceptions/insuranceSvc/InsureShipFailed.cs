using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.insuranceSvc
{
    public class InsureShipFailed : UserError
    {
        public InsureShipFailed(string reason) : base("InsureShipFailed",
            new PyDictionary {["reason"] = reason})
        {
        }
    }
}