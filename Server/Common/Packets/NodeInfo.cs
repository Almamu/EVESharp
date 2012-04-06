using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    public class NodeInfo
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

        public bool Decode(PyObject info)
        {
            if (info.Type != PyObjectType.ObjectData)
            {
                Log.Error("NodeInfo", "Wrong type for ObjectData");
                return false;
            }

            PyObjectData data = info.As<PyObjectData>();

            if (data.Name != "machoNet.nodeInfo")
            {
                Log.Error("NodeInfo", "Wrong object name, expected machoNet.nodeInfo but got " + data.Name);
                return false;
            }

            if (data.Arguments.Type != PyObjectType.Tuple)
            {
                Log.Error("NodeInfo", "Wrong type for ObjectData arguments, expected Tuple");
                return false;
            }

            PyTuple args = data.Arguments.As<PyTuple>();
            
            if (args.Items[0].Type != PyObjectType.Long)
            {
                Log.Error("NodeInfo", "Wrong type for tuple0 item0, expected int");
                return false;
            }

            nodeID = args.Items[0].As<PyInt>().Value;

            if (args.Items[1].Type != PyObjectType.List)
            {
                Log.Error("NodeInfo", "Wrong type for tuple0 item1, expected list");
                return false;
            }

            solarSystems = args.Items[1].As<PyList>();

            return true;
        }
    }
}
