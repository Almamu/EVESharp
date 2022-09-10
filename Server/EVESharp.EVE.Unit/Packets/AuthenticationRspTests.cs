using System.Net.Http;
using EVESharp.EVE.Data;
using EVESharp.EVE.Packets;
using EVESharp.Types;
using EVESharp.Types.Collections;
using NUnit.Framework;
using TestExtensions;

namespace EVESharp.EVE.Unit.Packets;

public class AuthenticationRspTests
{
    /// <summary>
    /// String "None" marshaled
    /// </summary>
    byte [] func_marshaled_code = {0x74, 0x04, 0x00, 0x00, 0x00, 0x4E, 0x6F, 0x6E, 0x65};
    
    [Test]
    public void AuthenticationRspBuild ()
    {
        PyDataType data = new AuthenticationRsp
        {
            serverChallenge         = "",
            func_marshaled_code     = func_marshaled_code,
            verification            = false,
            cluster_usercount       = 0,
            proxy_nodeid            = 6655,
            user_logonqueueposition = 1,
            challenge_responsehash  = "55087",

            macho_version = Version.MACHO_VERSION,
            boot_version  = Version.VERSION,
            boot_build    = Version.BUILD,
            boot_codename = Version.CODENAME,
            boot_region   = Version.REGION,
        };

        (PyString serverChallenge, PyTuple extra, PyDictionary context, PyDictionary info) =
            PyAssert.Tuple <PyString, PyTuple, PyDictionary, PyDictionary> (data);

        PyAssert.String (serverChallenge, "");
        (PyBuffer buffer, PyBool verification) = PyAssert.Tuple <PyBuffer, PyBool> (extra);

        PyAssert.Buffer (buffer, this.func_marshaled_code);
        PyAssert.Bool (verification, false);

        PyAssert.DictInteger (info, "macho_version", Version.MACHO_VERSION);
        PyAssert.DictDecimal (info, "boot_version", Version.VERSION);
        PyAssert.DictInteger (info, "boot_build", Version.BUILD);
        PyAssert.DictString (info, "boot_codename", Version.CODENAME);
        PyAssert.DictString (info, "boot_region",   Version.REGION);
        PyAssert.DictInteger (info, "cluster_usercount",       0);
        PyAssert.DictInteger (info, "proxy_nodeid",            6655);
        PyAssert.DictInteger (info, "user_logonqueueposition", 1);
        PyAssert.DictString (info, "challenge_responsehash", "55087");
    }
    
    [Test]
    public void AuthenticationRspParse ()
    {
        PyDataType data = new AuthenticationRsp
        {
            serverChallenge         = "",
            func_marshaled_code     = func_marshaled_code,
            verification            = false,
            cluster_usercount       = 0,
            proxy_nodeid            = 6655,
            user_logonqueueposition = 1,
            challenge_responsehash  = "55087",

            macho_version = Version.MACHO_VERSION,
            boot_version  = Version.VERSION,
            boot_build    = Version.BUILD,
            boot_codename = Version.CODENAME,
            boot_region   = Version.REGION,
        };

        AuthenticationRsp rsp = data;
        Assert.AreEqual ("",                       rsp.serverChallenge);
        Assert.AreEqual (this.func_marshaled_code, rsp.func_marshaled_code);
        Assert.AreEqual (false,                    rsp.verification);
        Assert.AreEqual (0,                        rsp.cluster_usercount);
        Assert.AreEqual (6655,                     rsp.proxy_nodeid);
        Assert.AreEqual (1,                        rsp.user_logonqueueposition);
        Assert.AreEqual ("55087",                  rsp.challenge_responsehash);
        Assert.AreEqual (Version.MACHO_VERSION,    rsp.macho_version);
        Assert.AreEqual (Version.VERSION,          rsp.boot_version);
        Assert.AreEqual (Version.BUILD,            rsp.boot_build);
        Assert.AreEqual (Version.CODENAME,         rsp.boot_codename);
        Assert.AreEqual (Version.REGION,           rsp.boot_region);
    }
}