using System.IO;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Packets;

public class HandshakeAck
{
    public PyList<PyObjectData> LiveUpdates = new PyList<PyObjectData>();
    public string               JIT            { get; init; } = "";
    public long                 UserID         { get; init; } = 0;
    public PyInteger            MaxSessionTime { get; init; } = null;
    public int                  UserType       { get; init; } = 1;
    public ulong                Role           { get; init; } = 0;
    public string               Address        { get; init; } = "";
    public PyBool               InDetention    { get; init; } = null;
    public PyList               ClientHashes   { get; init; } = new PyList();
    public long                 UserClientID   { get; init; } = 0;

    public static implicit operator PyDataType(HandshakeAck ack)
    {
        PyDictionary main = new PyDictionary
        {
            ["jit"]            = ack.JIT,
            ["userid"]         = ack.UserID,
            ["maxSessionTime"] = ack.MaxSessionTime,
            ["userType"]       = ack.UserType,
            ["role"]           = ack.Role,
            ["address"]        = ack.Address,
            ["inDetention"]    = ack.InDetention
        };

        PyDictionary result = new PyDictionary
        {
            ["live_updates"]  = ack.LiveUpdates,
            ["session_init"]  = main,
            ["client_hashes"] = ack.ClientHashes,
            ["user_clientid"] = ack.UserClientID
        };
            
        return result;
    }

    public static implicit operator HandshakeAck (PyDataType data)
    {
        if (data is not PyDictionary dict)
            throw new InvalidDataException ("HandshakeAck must be a dictionary");
        if (dict.TryGetValue ("session_init", out PyDictionary session) == false)
            throw new InvalidDataException ("HandshakeAck must have session initialization data");
        if (dict.TryGetValue ("user_clientid", out PyInteger userClientID) == false)
            throw new InvalidDataException ("HandshakeAck must have user client id");
        if (dict.TryGetValue ("client_hashes", out PyList clientHashes) == false)
            throw new InvalidDataException ("HandshakeAck must have client hashes");
        if (dict.TryGetValue ("live_updates", out PyList liveUpdatesUncasted) == false)
            throw new InvalidDataException ("HandshakeAck must have live updates");

        return new HandshakeAck ()
        {
            ClientHashes   = clientHashes,
            LiveUpdates    = liveUpdatesUncasted.GetEnumerable <PyObjectData> (),
            UserClientID   = userClientID,
            JIT            = session ["jit"] as PyString,
            UserID         = session ["userid"] as PyInteger,
            MaxSessionTime = session ["maxSessionTime"] as PyInteger,
            UserType       = session ["userType"] as PyInteger,
            Role           = session ["role"] as PyInteger,
            Address        = session ["address"] as PyString,
            InDetention    = session ["inDetention"] as PyBool
        };
    }
}