using System.Runtime.CompilerServices;
using EVESharp.EVE.Packets;
using EVESharp.Types;
using EVESharp.Types.Collections;
using NUnit.Framework;
using TestExtensions;

namespace EVESharp.EVE.Unit.Packets;

public class AuthenticationReqTests
{
    public static AuthenticationReq GetAuthenticationReq ()
    {
        return new AuthenticationReq
        {
            boot_build       = Data.Version.BUILD,
            boot_codename    = Data.Version.CODENAME,
            boot_region      = Data.Version.REGION,
            boot_version     = Data.Version.VERSION,
            macho_version    = Data.Version.MACHO_VERSION,
            user_affiliateid = 0,
            user_languageid  = "EN",
            user_name        = "Almamu",
            user_password    = "Password"
        };
    }
    
    [Test]
    public void AuthenticationReqBuild ()
    {
        PyDataType data = GetAuthenticationReq ();

        (PyNone _, PyDictionary dict) = PyAssert.Tuple <PyNone, PyDictionary> (data, true, 2);
        PyAssert.DictDecimal (dict, "boot_version", Data.Version.VERSION);
        PyAssert.DictString (dict, "boot_region", Data.Version.REGION);
        PyAssert.DictInteger (dict, "user_affiliateid", 0);
        PyAssert.DictInteger (dict, "macho_version",    Data.Version.MACHO_VERSION);
        PyAssert.DictString (dict, "boot_codename",   Data.Version.CODENAME);
        PyAssert.DictString (dict, "user_name",       "Almamu");
        PyAssert.DictString (dict, "user_languageid", "EN");
        PyObject password = PyAssert.DictObject (dict, "user_password");
        PyTuple  element  = PyAssert.Tuple <PyTuple> (password.Header, false, 1);
        (PyNone _, PyString pwd) = PyAssert.Tuple <PyNone, PyString> (element);

        PyAssert.String (pwd, "Password");
    }

    [Test]
    public void AuthenticationReqParse ()
    {
        PyDataType        data = GetAuthenticationReq ();
        AuthenticationReq req  = data;

        Assert.AreEqual (Data.Version.VERSION,       req.boot_version);
        Assert.AreEqual (Data.Version.REGION,        req.boot_region);
        Assert.AreEqual (Data.Version.MACHO_VERSION, req.macho_version);
        Assert.AreEqual (Data.Version.CODENAME,      req.boot_codename);
        Assert.AreEqual (0,                          req.user_affiliateid);
        Assert.AreEqual ("Almamu",                   req.user_name);
        Assert.AreEqual ("Password",                 req.user_password);
        Assert.AreEqual ("EN",                       req.user_languageid);
    }
}