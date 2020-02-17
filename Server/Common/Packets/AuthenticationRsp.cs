using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PythonTypes;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    public class AuthenticationRsp
    {
        public string serverChallenge = "";
        public byte[] func_marshaled_code = null;
        public bool verification = false;
        public PyDictionary context = new PyDictionary();
        public string challenge_responsehash = "";
        public int macho_version = 0;
        public double boot_version = 0.0;
        public int boot_build = 0;
        public string boot_codename = "";
        public string boot_region = "";
        public int cluster_usercount = 0;
        public int proxy_nodeid = 0;
        public int user_logonqueueposition = 0;

        public static implicit operator PyDataType(AuthenticationRsp rsp)
        {
            PyDictionary info = new PyDictionary();
            
            info["macho_version"] = rsp.macho_version;
            info["boot_version"] = rsp.boot_version;
            info["boot_build"] = rsp.boot_build;
            info["boot_codename"] = rsp.boot_codename;
            info["boot_region"] = rsp.boot_region;
            info["cluster_usercount"] = rsp.cluster_usercount;
            info["proxy_nodeid"] = rsp.proxy_nodeid;
            info["user_logonqueueposition"] = rsp.user_logonqueueposition;
            info["challenge_responsehash"] = rsp.challenge_responsehash;

            PyTuple extra = new PyTuple(new PyDataType[]
            {
                rsp.func_marshaled_code,
                rsp.verification
            });

            return new PyTuple(new PyDataType[]
            {
                rsp.serverChallenge,
                extra, rsp.context,
                info
            });
        }
    }
}
