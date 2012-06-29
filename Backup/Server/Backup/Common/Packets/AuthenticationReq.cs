using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    public class AuthenticationReq
    {
        public double boot_version = 0.0;
        public string boot_region = "";
        public string user_password = "";
        public int user_affiliateid = 0;
        public string user_password_hash = "";
        public int macho_version = 0;
        public string boot_codename = "";
        public int boot_build = 0;
        public string user_name = "";
        public string user_languageid = "";

        public bool Decode(PyObject data)
        {
            if (data.Type != PyObjectType.Tuple)
            {
                Log.Error("AuthenticationReq", "Wrong type");
                return false;
            }

            PyTuple tmp = data.As<PyTuple>();

            if (tmp.Items.Count != 2)
            {
                Log.Error("AuthenticationReq", "Wrong size, expected 2 but got " + tmp.Items.Count);
                return false;
            }

            if (tmp.Items[0].Type != PyObjectType.String)
            {
                Log.Error("AuthenticationReq", "Wrong type for item 1");
                return false;
            }

            if (tmp.Items[1].Type != PyObjectType.Dict)
            {
                Log.Error("AuthenticationReq", "Wrong type for item 2");
                return false;
            }

            PyDict info = tmp.Items[1].As<PyDict>();

            if (info.Contains("boot_version") == false)
            {
                Log.Error("AuthenticationReq", "Dict item 1 doesnt has key boot_version");
                return false;
            }

            boot_version = info.Get("boot_version").As<PyFloat>().Value;

            if (info.Contains("boot_region") == false)
            {
                Log.Error("AuthenticationReq", "Dict item 1 doesnt has key boot_region");
                return false;
            }

            boot_region = info.Get("boot_region").As<PyString>().Value;

            if (info.Contains("user_password") == false)
            {
                Log.Error("AuthenticationReq", "Dict item 1 doesnt has key user_password");
                return false;
            }

            if (info.Get("user_password").Type == PyObjectType.None)
            {
                user_password = null;
            }
            else
            {
                // user_password = info.Get("user_password").As<PyString>().Value;
                PyObjectEx obj = info.Get("user_password").As<PyObjectEx>();
                user_password = obj.Header.As<PyTuple>().Items[0].As<PyTuple>().Items[1].As<PyString>().Value;
            }

            if (info.Contains("user_affiliateid") == false)
            {
                Log.Error("AuthenticationReq", "Dict item 1 doesnt has key user_affiliateid");
                return false;
            }

            user_affiliateid = info.Get("user_affiliateid").As<PyInt>().Value;

            if (info.Contains("user_password_hash") == false)
            {
                Log.Error("AuthenticationReq", "Dict item 1 doesnt has key user_password_hash");
                return false;
            }

            if (info.Get("user_password_hash").Type == PyObjectType.None)
            {
                user_password_hash = null;
            }
            else
            {
                user_password_hash = info.Get("user_password_hash").As<PyString>().Value;
            }

            if (info.Contains("macho_version") == false)
            {
                Log.Error("AuthenticationReq", "Dict item 1 doesnt has key macho_version");
                return false;
            }

            macho_version = info.Get("macho_version").As<PyInt>().Value;

            if (info.Contains("boot_codename") == false)
            {
                Log.Error("AuthenticationReq", "Dict item 1 doesnt has key boot_codename");
                return false;
            }

            boot_codename = info.Get("boot_codename").As<PyString>().Value;

            if (info.Contains("boot_build") == false)
            {
                Log.Error("AuthenticationReq", "Dict item 1 doesnt has key boot_build");
                return false;
            }

            boot_build = info.Get("boot_build").As<PyInt>().Value;

            if (info.Contains("user_name") == false)
            {
                Log.Error("AuthenticationReq", "Dict item 1 doesnt has key user_name");
                return false;
            }

            user_name = info.Get("user_name").As<PyString>().Value;

            if (info.Contains("user_languageid") == false)
            {
                Log.Error("AuthenticationReq", "Dict item 1 doesnt has key user_languageid");
                return false;
            }

            user_languageid = info.Get("user_languageid").As<PyString>().Value;

            return true;
        }
    }
}
