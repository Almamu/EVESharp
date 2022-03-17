using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets
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
        public long proxy_nodeid = 0;
        public int user_logonqueueposition = 0;

        public static implicit operator PyDataType(AuthenticationRsp rsp)
        {
            PyDictionary info = new PyDictionary
            {
                ["macho_version"] = rsp.macho_version,
                ["boot_version"] = rsp.boot_version,
                ["boot_build"] = rsp.boot_build,
                ["boot_codename"] = rsp.boot_codename,
                ["boot_region"] = rsp.boot_region,
                ["cluster_usercount"] = rsp.cluster_usercount,
                ["proxy_nodeid"] = rsp.proxy_nodeid,
                ["user_logonqueueposition"] = rsp.user_logonqueueposition,
                ["challenge_responsehash"] = rsp.challenge_responsehash
            };

            PyTuple extra = new PyTuple(2)
            {
                [0] = rsp.func_marshaled_code,
                [1] = rsp.verification
            };

            return new PyTuple(4)
            {
                [0] = rsp.serverChallenge,
                [1] = extra,
                [2] = rsp.context,
                [3] = info
            };
        }
    }
}