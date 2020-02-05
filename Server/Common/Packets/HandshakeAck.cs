using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class HandshakeAck : Encodeable
    {
        public PyList live_updates = new PyList();
        public string jit = "";
        public int userid = 0;
        public PyNone maxSessionTime = new PyNone();
        public int userType = 1;
        public int role = 0;
        public string address = "";
        public PyNone inDetention = new PyNone();
        public PyList client_hashes = new PyList();
        public int user_clientid = 0;

        public PyObject Encode()
        {
            PyDict res = new PyDict();

            res.Set("live_updated", live_updates);

            PyDict main = new PyDict();

            main.Set("jit", new PyString(jit));
            main.Set("userid", new PyInt(userid));
            main.Set("maxSessionTime", maxSessionTime);
            main.Set("userType", new PyInt(userType));
            main.Set("role", new PyInt(role));
            main.Set("address", new PyString(address));
            main.Set("inDetention", inDetention);

            res.Set("session_init", main);
            res.Set("client_hashes", client_hashes);
            res.Set("user_clientid", new PyInt(user_clientid));

            return res.As<PyObject>();
        }
    }
}
