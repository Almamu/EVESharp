using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    public class HandshakeAck
    {
        public PyList live_updates = new PyList();
        public string jit = "";
        public long userid = 0;
        public PyNone maxSessionTime = new PyNone();
        public int userType = 1;
        public int role = 0;
        public string address = "";
        public PyNone inDetention = new PyNone();
        public PyList client_hashes = new PyList();
        public long user_clientid = 0;

        public static implicit operator PyDataType(HandshakeAck ack)
        {
            PyDictionary main = new PyDictionary();

            main["jit"] = ack.jit;
            main["userid"] = ack.userid;
            main["maxSessionTime"] = ack.maxSessionTime;
            main["userType"] = ack.userType;
            main["role"] = ack.role;
            main["address"] = ack.address;
            main["inDetention"] = ack.inDetention;

            PyDictionary result = new PyDictionary();

            result["live_updates"] = ack.live_updates;
            result["session_init"] = main;
            result["client_hashes"] = ack.client_hashes;
            result["user_clientid"] = ack.user_clientid;

            return result;
        }
    }
}