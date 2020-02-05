using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class AuthenticationReq : Decodeable
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

        public void Decode(PyObject data)
        {
            if (data.Type != PyObjectType.Tuple)
            {
                throw new Exception("Wrong type");
            }

            PyTuple tmp = data.As<PyTuple>();

            if (tmp.Items.Count != 2)
            {
                throw new Exception($"Wrong size, expected 2 but got {tmp.Items.Count}");
            }

            if (tmp.Items[0].Type != PyObjectType.String)
            {
                throw new Exception($"Expected string for item 1 but got {tmp.Items[0].Type}");
            }

            if (tmp.Items[1].Type != PyObjectType.Dict)
            {
                throw new Exception($"Expected string for item 2 but got {tmp.Items[1].Type}");
            }

            PyDict info = tmp.Items[1].As<PyDict>();

            if (info.Contains("boot_version") == false)
            {
                throw new Exception("PyDict item 1 doesn't have the key 'boot_version'");
            }

            boot_version = info.Get("boot_version").As<PyFloat>().Value;

            if (info.Contains("boot_region") == false)
            {
                throw new Exception("PyDict item 1 doesn't have the key 'boot_region'");
            }

            boot_region = info.Get("boot_region").As<PyString>().Value;

            if (info.Contains("user_password") == false)
            {
                throw new Exception("PyDict item 1 doesn't have the key 'user_password'");
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
                throw new Exception("PyDict item 1 doesn't have the key 'user_affiliateid'");
            }

            user_affiliateid = info.Get("user_affiliateid").As<PyInt>().Value;

            if (info.Contains("user_password_hash") == false)
            {
                throw new Exception("PyDict item 1 doesn't have the key 'user_password_hash'");
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
                throw new Exception("PyDict item 1 doesn't have the key 'macho_version'");
            }

            macho_version = info.Get("macho_version").As<PyInt>().Value;

            if (info.Contains("boot_codename") == false)
            {
                throw new Exception("PyDict item 1 doesn't have the key 'boot_codename'");
            }

            boot_codename = info.Get("boot_codename").As<PyString>().Value;

            if (info.Contains("boot_build") == false)
            {
                throw new Exception("PyDict item 1 doesn't have the key 'boot_build'");
            }

            boot_build = info.Get("boot_build").As<PyInt>().Value;

            if (info.Contains("user_name") == false)
            {
                throw new Exception("PyDict item 1 doesn't have the key 'user_name'");
            }

            user_name = info.Get("user_name").As<PyString>().Value;

            if (info.Contains("user_languageid") == false)
            {
                throw new Exception("PyDict item 1 doesn't have the key user_languageid'");
            }

            user_languageid = info.Get("user_languageid").As<PyString>().Value;
        }
    }
}
