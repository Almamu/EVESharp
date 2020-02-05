using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class NodeInfo : Encodeable, Decodeable
    {
        public int nodeID = 0;
        public PyList solarSystems = new PyList();

        public PyObject Encode()
        {
            PyTuple packet = new PyTuple();

            packet.Items.Add(new PyInt(nodeID));
            packet.Items.Add(solarSystems);

            return new PyObjectData("machoNet.nodeInfo", packet);
        }

        public void Decode(PyObject info)
        {
            if (info.Type != PyObjectType.ObjectData)
            {
                throw new Exception($"Expected container of type ObjectData but got {info.Type}");
            }

            PyObjectData data = info.As<PyObjectData>();

            if (data.Name != "machoNet.nodeInfo")
            {
                throw new Exception($"Expected container with typeName 'machoNet.nodeInfo but got {data.Name}");
            }

            if (data.Arguments.Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected arguments of type Tuple but got {data.Arguments.Type}");
            }

            PyTuple args = data.Arguments.As<PyTuple>();
            
            if (args.Items[0].Type != PyObjectType.Long)
            {
                throw new Exception($"Expected first argument of type Long but got {args.Items[0].Type}");
            }

            nodeID = args.Items[0].As<PyInt>().Value;

            if (args.Items[1].Type != PyObjectType.List)
            {
                throw new Exception($"Expected second argument of type List but got {args.Items[1].Type}");
            }

            solarSystems = args.Items[1].As<PyList>();
        }
    }
}
