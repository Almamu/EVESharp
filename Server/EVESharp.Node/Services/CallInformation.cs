using EVESharp.EVE.Services;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.Server.Shared.Transports;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services;

public class CallInformation : ServiceCall
{
    public IMachoNet                           MachoNet            { get; init; }
    public PyDictionary <PyString, PyDataType> ResultOutOfBounds   { get; init; }
    public BoundServiceManager                 BoundServiceManager { get; init; }
    public ServiceManager                      ServiceManager      { get; init; }
    public MachoTransport                      Transport           { get; init; }
}