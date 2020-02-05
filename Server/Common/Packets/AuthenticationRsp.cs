using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class AuthenticationRsp : Encodeable
    {
        public string serverChallenge = "";
        public byte[] func_marshaled_code = null;
        public bool verification = false;
        public PyDict context = null;
        public string challenge_responsehash = "";
        public int macho_version = 0;
        public double boot_version = 0.0;
        public int boot_build = 0;
        public string boot_codename = "";
        public string boot_region = "";
        public int cluster_usercount = 0;
        public int proxy_nodeid = 0;
        public int user_logonqueueposition = 0;

        public PyObject Encode()
        {
            PyTuple res = new PyTuple();

            res.Items.Add(new PyString(serverChallenge));

            PyTuple extra = new PyTuple();

            extra.Items.Add(new PyBuffer(func_marshaled_code));
            extra.Items.Add(new PyBool(verification));

            res.Items.Add(extra);

            if (context == null)
            {
                res.Items.Add(new PyDict());
            }
            else
            {
                res.Items.Add(context);
            }

            PyDict info = new PyDict();

            info.Set("macho_version", new PyInt(macho_version));
            info.Set("boot_version", new PyFloat(boot_version));
            info.Set("boot_build", new PyInt(boot_build));
            info.Set("boot_codename", new PyString(boot_codename));
            info.Set("boot_region", new PyString(boot_region));
            info.Set("cluster_usercount", new PyInt(cluster_usercount));
            info.Set("proxy_nodeid", new PyInt(proxy_nodeid));
            info.Set("user_logonqueueposition", new PyInt(user_logonqueueposition));
            info.Set("challenge_responsehash", new PyString(challenge_responsehash));

            res.Items.Add(info);

            return res.As<PyObject>();
        }
    }
}
