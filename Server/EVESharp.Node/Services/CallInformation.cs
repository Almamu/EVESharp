using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Services;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services;

public class CallInformation : ServiceCall
{
    public IMachoNet                           MachoNet            { get; init; }
    public PyDictionary <PyString, PyDataType> ResultOutOfBounds   { get; init; }
    public BoundServiceManager                 BoundServiceManager { get; init; }
    public ServiceManager                      ServiceManager      { get; init; }
    public IMachoTransport                     Transport           { get; init; }
}