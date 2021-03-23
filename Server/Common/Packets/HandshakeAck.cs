using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    public class HandshakeAck
    {
        public PyList<PyObjectData> live_updates = new PyList<PyObjectData>();
        public string jit = "";
        public long userid = 0;
        public PyInteger maxSessionTime = null;
        public int userType = 1;
        public int role = 0;
        public string address = "";
        public PyBool inDetention = null;
        public PyList client_hashes = new PyList();
        public long user_clientid = 0;

        public static implicit operator PyDataType(HandshakeAck ack)
        {
            PyDictionary main = new PyDictionary
            {
                ["jit"] = ack.jit,
                ["userid"] = ack.userid,
                ["maxSessionTime"] = ack.maxSessionTime,
                ["userType"] = ack.userType,
                ["role"] = ack.role,
                ["address"] = ack.address,
                ["inDetention"] = ack.inDetention
            };

            PyDictionary result = new PyDictionary
            {
                ["live_updates"] = ack.live_updates,
                ["session_init"] = main,
                ["client_hashes"] = ack.client_hashes,
                ["user_clientid"] = ack.user_clientid
            };
            
            return result;
        }
    }
}