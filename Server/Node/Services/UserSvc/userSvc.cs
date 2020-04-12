using Common.Logging;
using Common.Services;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Services.UserSvc
{
    public class userSvc : Service
    {
        public userSvc(ServiceManager manager) : base(manager)
        {

        }

        public PyList GetRedeemTokens(PyDictionary namedPayload, Client client)
        {
            return new PyList();
        }
    }
}
