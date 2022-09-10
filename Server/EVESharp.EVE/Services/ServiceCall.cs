using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types.Network;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Services;

public abstract class ServiceCall
{
    public PyAddress Source { get; init; }
    public PyAddress Destination { get; init; }
    public Session Session { get; init; }
    public PyTuple Payload { get; init; }
    public PyDictionary NamedPayload { get; init; }
    public int CallID { get; init; }
    public int NodeID { get; init; }
}