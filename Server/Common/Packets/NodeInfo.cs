using System.IO;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    public class NodeInfo
    {
        private const string TYPE_NAME = "machoNet.nodeInfo";

        public long nodeID = 0;
        public PyList solarSystems = new PyList();

        public static implicit operator PyDataType(NodeInfo info)
        {
            return new PyObjectData(TYPE_NAME, new PyTuple(2)
                {
                    [0] = info.nodeID,
                    [1] = info.solarSystems
                }
            );
        }

        public static implicit operator NodeInfo(PyDataType info)
        {
            if (info is PyObjectData == false)
                throw new InvalidDataException($"Expected container of type ObjectData");

            PyObjectData data = info as PyObjectData;

            if (data.Name != TYPE_NAME)
                throw new InvalidDataException($"Expected ObjectData of type {TYPE_NAME} but got {data.Name}");

            PyTuple arguments = data.Arguments as PyTuple;

            NodeInfo result = new NodeInfo
            {
                nodeID = arguments[0] as PyInteger,
                solarSystems = arguments[1] as PyList
            };


            return result;
        }
    }
}