using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    public class PyException
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

        public bool Decode(PyObject data)
        {
            if (data.Type != PyObjectType.ObjectEx)
            {
                Log.Error("PyException", "Wrong container type");
                return false;
            }

            PyObjectEx p = data.As<PyObjectEx>();

            if (p.IsType2 == true)
            {
                Log.Error("PyException", "Wrong PyObjectEx type, expected Normal, but got Type2");
                return false;
            }

            if (p.Header.Type != PyObjectType.Tuple)
            {
                Log.Error("PyException", "Wrong item 1 type");
                return false;
            }

            PyTuple args = p.Header.As<PyTuple>();
            if (args.Items.Count != 3)
            {
                Log.Error("PyException", "Wrong tuple 1 item count, expected 3 but got " + args.Items.Count);
                return false;
            }

            if (args.Items[0].Type != PyObjectType.Token)
            {
                Log.Error("PyException", "Wrong tuple item 1 type");
                return false;
            }

            PyToken type = args.Items[0].As<PyToken>();
            exception_type = type.Token;

            if (exception_type.StartsWith("exceptions.") == false)
            {
                Log.Warning("PyException", "Trying to decode a non-exception packet: " + exception_type);
                return false;
            }

            if (args.Items[1].Type != PyObjectType.Tuple)
            {
                Log.Error("PyException", "Wrong tuple item 2 type");
                return false;
            }

            PyTuple msg = args.Items[1].As<PyTuple>();

            if (msg.Items.Count != 1)
            {
                Log.Error("PyException", "Wrong item 2 tuple count, expected 1 but got " + msg.Items.Count);
                return false;
            }

            if (msg.Items[0].Type != PyObjectType.String)
            {
                Log.Error("PyException", "Wrong tuple 2 item 1 type");
                return false;
            }

            PyString msg_data = msg.Items[0].As<PyString>();

            message = msg_data.Value;

            if (args.Items[2].Type != PyObjectType.Dict)
            {
                Log.Error("PyException", "Wrong tuple 1 item 3 type");
                return false;
            }

            PyDict info = args.Items[2].As<PyDict>();

            if (info.Contains("origin") == false)
            {
                Log.Error("PyException", "Dict item 1 doesnt has key origin");
                return false;
            }

            origin = info.Get("origin").As<PyString>().Value;

            if (info.Contains("reasonArgs") == false)
            {
                Log.Error("PyException", "Dict item 1 doesn has key reasonArgs");
                return false;
            }

            reasonArgs = info.Get("reasonArgs").As<PyDict>();

            if (info.Contains("clock") == false)
            {
                Log.Error("PyException", "Dict item 1 doesnt has key clock");
                return false;
            }

            clock = info.Get("clock").IntValue;

            if (info.Contains("loggedOnUserCount") == false)
            {
                Log.Error("PyException", "Dict item 1 doesnt has key loggedOnUserCount");
                return false;
            }

            loggedOnUserCount = info.Get("loggedOnUserCount");

            if (info.Contains("region") == false)
            {
                Log.Error("PyException", "Dict item 1 doesnt has key region");
                return false;
            }

            region = info.Get("region").As<PyString>().Value;

            if (info.Contains("reason") == false)
            {
                Log.Error("PyException", "Dict item 1 doesnt has key reason");
                return false;
            }

            reason = info.Get("reason").As<PyString>().Value;

            if(info.Contains("version") == false)
            {
                Log.Error("PyException", "Dict item 1 doesnt has key version");
                return false;
            }

            version = info.Get("version").As<PyFloat>().Value;

            if (info.Contains("build") == false)
            {
                Log.Error("PyException", "Dict item 1 doesnt has key build");
                return false;
            }

            build = info.Get("build").As<PyInt>().Value;

            if (info.Contains("reasonCode") == false)
            {
                Log.Error("PyException", "Dict item 1 doesnt has key reasonCode");
                return false;
            }

            reasonCode = info.Get("reasonCode").StringValue;

            if (info.Contains("codename") == false)
            {
                Log.Error("PyException", "Dict item 1 doesnt has key codename");
                return false;
            }

            codename = info.Get("codename").As<PyString>().Value;

            if (info.Contains("machoVersion") == false)
            {
                Log.Error("PyException", "Dict item 1 doesnt has key machoVersion");
                return false;
            }

            machoVersion = info.Get("machoVersion").As<PyInt>().Value;

            return true;
        }
    }
}
