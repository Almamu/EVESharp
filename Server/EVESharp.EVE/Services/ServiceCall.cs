using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;

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