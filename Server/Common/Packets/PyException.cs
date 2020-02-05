using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class PyException : Decodeable, Encodeable
    {
        public string exception_type = "";
        public string message = "";
        public string origin = "";
        public PyDict reasonArgs = new PyDict();
        public long clock = 0;
        public PyObject loggedOnUserCount = null;
        public string region = "";
        public string reason = "";
        public double version = 0.0;
        public int build = 0;
        public string reasonCode = "";
        public string codename = "";
        public int machoVersion = 0;

        public PyObject Encode()
        {
            PyTuple header = new PyTuple();
            PyTuple args = new PyTuple();
            PyToken exception = new PyToken(exception_type);
            PyDict keywords = new PyDict();

            args.Items.Add(new PyString(reason));

            keywords.Set("reasonArgs", reasonArgs);
            keywords.Set("clock", new PyLongLong(clock));
            keywords.Set("region", new PyString(region));
            keywords.Set("reason", new PyString(reason));
            keywords.Set("version", new PyFloat(version));
            keywords.Set("build", new PyInt(build));
            keywords.Set("codename", new PyString(codename));
            keywords.Set("machoVersion", new PyInt(machoVersion));

            header.Items.Add(exception);
            header.Items.Add(args);
            header.Items.Add(keywords);

            return new PyObjectEx(false, header);
        }

        public void Decode(PyObject data)
        {
            if (data.Type != PyObjectType.ObjectEx)
            {
                throw new Exception($"Expected container of type ObjectEx but got {data.Type}");
            }

            PyObjectEx p = data.As<PyObjectEx>();

            if (p.IsType2 == true)
            {
                throw new Exception($"Expected PyObjectEx to be of type 2 but got normal");
            }

            if (p.Header.Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected header to be of type Tuple but got {p.Header.Type}");
            }

            PyTuple args = p.Header.As<PyTuple>();
            
            if (args.Items.Count != 3)
            {
                throw new Exception($"Expected header to have 3 items but got {args.Items.Count}");
            }

            if (args.Items[0].Type != PyObjectType.Token)
            {
                throw new Exception($"Expected first argument to be of type Token but got {args.Items[0].Type}");
            }

            PyToken type = args.Items[0].As<PyToken>();
            exception_type = type.Token;

            if (exception_type.StartsWith("exceptions.") == false)
            {
                throw new Exception($"Trying to decode a non-exception packet: {exception_type}");
            }

            if (args.Items[1].Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected second argument to be of type Tuple but got {args.Items[1].Type}");
            }

            PyTuple msg = args.Items[1].As<PyTuple>();

            if (msg.Items.Count != 1)
            {
                throw new Exception($"Expected second argument Tuple to have 1 element but got {msg.Items.Count}");
            }

            if (msg.Items[0].Type != PyObjectType.String)
            {
                throw new Exception($"Expected item 1 to be of type String but got {msg.Items[0].Type}");
            }

            PyString msg_data = msg.Items[0].As<PyString>();

            message = msg_data.Value;

            if (args.Items[2].Type != PyObjectType.Dict)
            {
                throw new Exception($"Expected third argument to be of type PyDict but got {args.Items[2].Type}");
            }

            PyDict info = args.Items[2].As<PyDict>();

            if (info.Contains("origin") == false)
            {
                throw new Exception("PyDict doesn't have the key 'origin'");
            }

            origin = info.Get("origin").As<PyString>().Value;

            if (info.Contains("reasonArgs") == false)
            {
                throw new Exception("PyDict doesn't have the key 'reasonArgs'");
            }

            reasonArgs = info.Get("reasonArgs").As<PyDict>();

            if (info.Contains("clock") == false)
            {
                throw new Exception("PyDict doesn't have the key 'clock'");
            }

            clock = info.Get("clock").IntValue;

            if (info.Contains("loggedOnUserCount") == false)
            {
                throw new Exception("PyDict doesn't have the key 'loggedOnUserCount'");
            }

            loggedOnUserCount = info.Get("loggedOnUserCount");

            if (info.Contains("region") == false)
            {
                throw new Exception("PyDict doesn't have the key 'region'");
            }

            region = info.Get("region").As<PyString>().Value;

            if (info.Contains("reason") == false)
            {
                throw new Exception("PyDict doesn't have the key 'reason'");
            }

            reason = info.Get("reason").As<PyString>().Value;

            if(info.Contains("version") == false)
            {
                throw new Exception("PyDict doesn't have the key 'version'");
            }

            version = info.Get("version").As<PyFloat>().Value;

            if (info.Contains("build") == false)
            {
                throw new Exception("PyDict doesn't have the key 'build'");
            }

            build = info.Get("build").As<PyInt>().Value;

            if (info.Contains("reasonCode") == false)
            {
                throw new Exception("PyDict doesn't have the key 'reasonCode'");
            }

            reasonCode = info.Get("reasonCode").StringValue;

            if (info.Contains("codename") == false)
            {
                throw new Exception("PyDict doesn't have the key 'codename'");
            }

            codename = info.Get("codename").As<PyString>().Value;

            if (info.Contains("machoVersion") == false)
            {
                throw new Exception("PyDict doesn't have the key 'machoVersion'");
            }

            machoVersion = info.Get("machoVersion").As<PyInt>().Value;
        }
    }
}
