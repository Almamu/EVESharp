using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets
{
    public class IdentificationReq
    {
        private const string TYPE_NAME = "macho.IdentificationReq";

        /// <summary>
        /// The address of the requesting node
        /// </summary>
        public string Address { get; init; }
        /// <summary>
        /// The nodeID of the requesting node
        /// </summary>
        public int NodeID { get; init; }

        public static implicit operator PyDataType(IdentificationReq info)
        {
            return new PyObjectData(TYPE_NAME, new PyTuple(2)
                {
                    [0] = info.Address,
                    [1] = info.NodeID
                }
            );
        }

        public static implicit operator IdentificationReq(PyDataType info)
        {
            if (info is PyObjectData == false)
                throw new InvalidDataException($"Expected container of type ObjectData");

            PyObjectData data = info as PyObjectData;

            if (data.Name != TYPE_NAME)
                throw new InvalidDataException($"Expected ObjectData of type {TYPE_NAME} but got {data.Name}");

            PyTuple arguments = data.Arguments as PyTuple;

            IdentificationReq result = new IdentificationReq
            {
                Address = arguments[0] as PyString,
                NodeID = arguments[1] as PyInteger
            };
            
            return result;
        }   
    }
}
