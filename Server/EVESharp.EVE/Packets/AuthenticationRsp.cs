using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets;

public class AuthenticationRsp
{
    public string       serverChallenge         = "";
    public byte[]       func_marshaled_code     = null;
    public bool         verification            = false;
    public PyDictionary context                 = new PyDictionary();
    public string       challenge_responsehash  = "";
    public int          macho_version           = 0;
    public double       boot_version            = 0.0;
    public int          boot_build              = 0;
    public string       boot_codename           = "";
    public string       boot_region             = "";
    public int          cluster_usercount       = 0;
    public long         proxy_nodeid            = 0;
    public int          user_logonqueueposition = 0;

    public static implicit operator PyDataType(AuthenticationRsp rsp)
    {
        PyDictionary info = new PyDictionary
        {
            ["macho_version"]           = rsp.macho_version,
            ["boot_version"]            = rsp.boot_version,
            ["boot_build"]              = rsp.boot_build,
            ["boot_codename"]           = rsp.boot_codename,
            ["boot_region"]             = rsp.boot_region,
            ["cluster_usercount"]       = rsp.cluster_usercount,
            ["proxy_nodeid"]            = rsp.proxy_nodeid,
            ["user_logonqueueposition"] = rsp.user_logonqueueposition,
            ["challenge_responsehash"]  = rsp.challenge_responsehash
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

    public static implicit operator AuthenticationRsp (PyDataType data)
    {
        if (data is not PyTuple tuple || tuple.Count != 4)
            throw new InvalidDataException ("AuthenticationRsp container must be a tuple with 4 elements");
        if (tuple [1] is not PyTuple extra || extra.Count != 2)
            throw new InvalidDataException ("AuthenticationRsp verification must be a tuple of 2 elements");
        if (tuple [3] is not PyDictionary dict)
            throw new InvalidDataException ("AuthenticationRsp information must be a dictionary");

        return new AuthenticationRsp ()
        {
            serverChallenge = tuple [0] as PyString,
            func_marshaled_code = extra [0] as PyBuffer,
            verification = extra [1] as PyBool,
            context = tuple [2] as PyDictionary,
            macho_version = dict ["macho_version"] as PyInteger,
            boot_version = dict ["boot_version"] as PyDecimal,
            boot_build = dict ["boot_build"] as PyInteger,
            boot_codename = dict ["boot_codename"] as PyString,
            boot_region = dict ["boot_region"] as PyString,
            cluster_usercount = dict ["cluster_usercount"] as PyInteger,
            proxy_nodeid = dict ["proxy_nodeid"] as PyInteger,
            user_logonqueueposition = dict ["user_logonqueueposition"] as PyInteger,
            challenge_responsehash = dict ["challenge_responsehash"] as PyString
        };
    }
}