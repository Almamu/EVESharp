using EVESharp.EVE;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    public class CallInformation : ServiceCall
    {
        public MachoNet MachoNet { get; init; }
        public PyDictionary<PyString,PyDataType> ResutOutOfBounds { get; init; }
    }
}