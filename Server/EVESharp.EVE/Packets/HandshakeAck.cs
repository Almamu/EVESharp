using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

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
}