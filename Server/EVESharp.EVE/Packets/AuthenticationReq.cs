using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets;

public class AuthenticationReq
{
    public double boot_version       = 0.0;
    public string boot_region        = "";
    public string user_password      = "";
    public int    user_affiliateid   = 0;
    public string user_password_hash = "";
    public int    macho_version      = 0;
    public string boot_codename      = "";
    public int    boot_build         = 0;
    public string user_name          = "";
    public string user_languageid    = "";

    public static implicit operator AuthenticationReq(PyDataType data)
    {
        PyTuple tuple = data as PyTuple;

        if (tuple.Count != 2)
            throw new InvalidDataException($"Expected a tuple of two elements");

        PyDictionary info = tuple[1] as PyDictionary;

        AuthenticationReq result = new AuthenticationReq
        {
            boot_version     = info["boot_version"] as PyDecimal,
            boot_region      = info["boot_region"] as PyString,
            user_affiliateid = info["user_affiliateid"] as PyInteger,
            macho_version    = info["macho_version"] as PyInteger,
            boot_codename    = info["boot_codename"] as PyString,
            boot_build       = info["boot_build"] as PyInteger,
            user_name        = info["user_name"] as PyString,
            user_languageid  = info["user_languageid"] as PyString
        };
            
        if (info["user_password_hash"] is null)
            result.user_password_hash = null;
        else
            result.user_password_hash = info["user_password_hash"] as PyString;

        if (info["user_password"] is null)
            result.user_password = null;
        else if (info["user_password"] is PyObject)
            result.user_password = ((info["user_password"] as PyObject).Header[0] as PyTuple)[1] as PyString;

        return result;
    }
}