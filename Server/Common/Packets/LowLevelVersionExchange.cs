using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class LowLevelVersionExchange : Encodeable, Decodeable
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

        public PyObject Encode()
        {
            PyTuple res = new PyTuple();

            res.Items.Add(new PyInt(birthday));
            res.Items.Add(new PyInt(machoVersion));

            if (this.isNode)
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

        public void Decode(PyObject data)
        {
            isNode = false;

            if (data.Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected container of type Tuple but got {data.Type}");
            }

            PyTuple tmp = data.As<PyTuple>();

            if (tmp.Items.Count != 6)
            {
                throw new Exception($"Expected container with 6 elements but got {tmp.Items.Count}");
            }

            PyObject birth = tmp.Items[0];

            if ( (birth.Type != PyObjectType.IntegerVar) && (birth.Type != PyObjectType.LongLong) && (birth.Type != PyObjectType.Long))
            {
                throw new Exception($"Expected a birthday of type Long, LongLong or Integer but got {birth.Type}");
            }

            birthday = birth.As<PyInt>().Value;

            PyObject macho = tmp.Items[1];

            if ((macho.Type != PyObjectType.IntegerVar) && (macho.Type != PyObjectType.LongLong) && (macho.Type != PyObjectType.Long))
            {
                throw new Exception($"Expected a machoVersion of type Long, LongLong or Integer but got {macho.Type}");
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
            else if (users.Type == PyObjectType.String)
            {
                isNode = true;
                nodeIdentifier = users.As<PyString>().Value;
            }
            else
            {
                throw new Exception($"Wrong type for usercount/node identifier, got {users.Type}");
            }

            PyObject ver = tmp.Items[3];

            if (ver.Type != PyObjectType.Float)
            {
                throw new Exception($"Expected a version of type Float but got {ver.Type}");
            }

            version = ver.As<PyFloat>().Value;

            PyObject b = tmp.Items[4];

            if ((b.Type != PyObjectType.IntegerVar) && (b.Type != PyObjectType.LongLong) && (b.Type != PyObjectType.Long))
            {
                throw new Exception($"Expected a build of type Long, LongLong or Integer but got {b.Type}");
            }

            build = b.As<PyInt>().Value;

            PyObject code = tmp.Items[5];

            if (code.Type != PyObjectType.String)
            {
                throw new Exception($"Expected a codename of type String but got {code.Type}");
            }

            codename = code.As<PyString>().Value;
        }
    }
}
