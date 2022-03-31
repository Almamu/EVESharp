using EVESharp.EVE;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    public class CallInformation : ServiceCall
    {
        public IMachoNet MachoNet { get; init; }
        public PyDictionary<PyString,PyDataType> ResultOutOfBounds { get; init; }
        public BoundServiceManager BoundServiceManager { get; init; }
        public ServiceManager ServiceManager { get; init; }
        public MachoTransport Transport { get; init; }
    }
}