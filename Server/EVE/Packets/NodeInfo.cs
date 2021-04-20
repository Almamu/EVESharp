using System.IO;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace EVE.Packets
{
    public class NodeInfo
    {
        private const string TYPE_NAME = "machoNet.nodeInfo";

        public long NodeID { get; init; }
        public PyList SolarSystems { get; init; } = new PyList();

        public static implicit operator PyDataType(NodeInfo info)
        {
            return new PyObjectData(TYPE_NAME, new PyTuple(2)
                {
                    [0] = info.NodeID,
                    [1] = info.SolarSystems
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
                NodeID = arguments[0] as PyInteger,
                SolarSystems = arguments[1] as PyList
            };
            
            return result;
        }
    }
}