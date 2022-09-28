using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types.Network;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Network.Services;

public class ServiceCall
{
    public PyAddress                           Source              { get; init; }
    public PyAddress                           Destination         { get; init; }
    public Session                             Session             { get; init; }
    public PyTuple                             Payload             { get; init; }
    public PyDictionary                        NamedPayload        { get; init; }
    public int                                 CallID              { get; init; }
    public int                                 NodeID              { get; init; }
    public IMachoNet                           MachoNet            { get; init; }
    public PyDictionary <PyString, PyDataType> ResultOutOfBounds   { get; init; }
    public IBoundServiceManager                BoundServiceManager { get; init; }
    public IServiceManager<string>             ServiceManager      { get; init; }
    public IMachoTransport                     Transport           { get; init; }
}