using System.IO;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Packets;

public class AuthenticationReq
{
    public double boot_version;
    public string boot_region   = "";
    public string user_password = "";
    public int    user_affiliateid;
    public string user_password_hash = null;
    public int    macho_version;
    public string boot_codename = "";
    public int    boot_build;
    public string user_name       = "";
    public string user_languageid = "";

    public static implicit operator AuthenticationReq(PyDataType data)
    {
        PyTuple tuple = data as PyTuple;

        if (tuple.Count != 2)
            throw new InvalidDataException("Expected a tuple of two elements");

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

    public static implicit operator PyDataType (AuthenticationReq req)
    {
        return new PyTuple (2)
        {
            [0] = null, // random bytes, i guess for encryption?
            [1] = new PyDictionary()
            {
                ["boot_version"] = req.boot_version,
                ["boot_region"] = req.boot_region,
                ["user_affiliateid"] = req.user_affiliateid,
                ["macho_version"] = req.macho_version,
                ["boot_codename"] = req.boot_codename,
                ["boot_build"] = req.boot_build,
                ["user_name"] = req.user_name,
                ["user_languageid"] = req.user_languageid,
                ["user_password_hash"] = req.user_password_hash,
                ["user_password"] = req.user_password_hash is not null ? null :
                    new PyObject (
                        false, 
                        new PyTuple (1)
                        {
                            [0] = new PyTuple (2)
                            {
                                [0] = null, [1] = req.user_password
                            }
                        }
                    )
            }
        };
    }
}