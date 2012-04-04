using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    public class LowLevelVersionExchange
    {
        public int birthday = 0;
        public int machoVersion = 0;
        public int usercount = 0;
        public double version = 0.0;
        public int build = 0;
        public string codename = "";
        public string region = "";
        public string nodeIdentifier = "";
        public bool isNode = false; // 0-> Client, 1-> Node

        public PyTuple Encode(bool node)
        {
            PyTuple res = new PyTuple();

            res.Items.Add(new PyInt(birthday));
            res.Items.Add(new PyInt(machoVersion));

            if (node)
            {
                res.Items.Add(new PyString("Node"));
            }
            else
            {
                res.Items.Add(new PyInt(usercount));
            }

            res.Items.Add(new PyFloat(version));
            res.Items.Add(new PyInt(build));
            res.Items.Add(new PyString(codename + "@" + region));

            return res;
        }

        public bool Decode(PyObject data)
        {
            isNode = false;

            if (data.Type != PyObjectType.Tuple)
            {
                Log.Error("LowLevelVersionExchange", "Wrong type");
                return false;
            }

            PyTuple tmp = data.As<PyTuple>();

            if (tmp.Items.Count != 6)
            {
                Log.Error("LowLevelVersionExchange", "Wrong item count");
                return false;
            }

            PyObject birth = tmp.Items[0];

            if ( (birth.Type != PyObjectType.IntegerVar) && (birth.Type != PyObjectType.LongLong) && (birth.Type != PyObjectType.Long))
            {
                Log.Error("LowLevelVersionExchange", "Wrong type for birthday. Type: " + (uint)birth.Type);
                return false;
            }

            birthday = birth.As<PyInt>().Value;

            PyObject macho = tmp.Items[1];

            if ((macho.Type != PyObjectType.IntegerVar) && (macho.Type != PyObjectType.LongLong) && (macho.Type != PyObjectType.Long))
            {
                Log.Error("LowLevelVersionExchange", "Wrong type for machoVersion");
                return false;
            }

            machoVersion = macho.As<PyInt>().Value;

            PyObject users = tmp.Items[2];

            if (users.Type == PyObjectType.None)
            {
                usercount = 0;   
            }
            else if(users.Type == PyObjectType.Long)
            {
                usercount = users.As<PyInt>().Value;
            }
            else if (users.Type != PyObjectType.String)
            {
                isNode = true;
                nodeIdentifier = users.As<PyString>().Value;
            }
            else
            {
                Log.Error("LowLevelVersionExchange", "Wrong type for usercount");
                return false;
            }

            PyObject ver = tmp.Items[3];

            if (ver.Type != PyObjectType.Float)
            {
                Log.Error("LowLevelVersionExchange", "Wrong type for version");
                return false;
            }

            version = ver.As<PyFloat>().Value;

            PyObject b = tmp.Items[4];

            if ((b.Type != PyObjectType.IntegerVar) && (b.Type != PyObjectType.LongLong) && (b.Type != PyObjectType.Long))
            {
                Log.Error("LowLevelVersionExchange", "Wrong type for build");
                return false;
            }

            build = b.As<PyInt>().Value;

            PyObject code = tmp.Items[5];

            if (code.Type != PyObjectType.String)
            {
                Log.Error("LowLevelVersionExchange", "Wrong type for codename");
                return false;
            }

            codename = code.As<PyString>().Value;

            return true;
        }
    }
}
