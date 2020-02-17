using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PythonTypes;
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
            return new PyObjectData(TYPE_NAME, new PyTuple( new PyDataType[]
            {
                info.nodeID, info.solarSystems, 
            }));
        }

        public static implicit operator NodeInfo(PyDataType info)
        {
            if (info is PyObjectData == false)
                throw new InvalidDataException($"Expected container of type ObjectData");
            
            PyObjectData data = info as PyObjectData;
            
            if(data.Name != TYPE_NAME)
                throw new InvalidDataException($"Expected ObjectData of type {TYPE_NAME} but got {data.Name}");

            PyTuple arguments = data.Arguments as PyTuple;
            
            NodeInfo result = new NodeInfo();

            result.nodeID = arguments[0] as PyInteger;
            result.solarSystems = arguments[1] as PyList;

            return result;
        }
    }
}
