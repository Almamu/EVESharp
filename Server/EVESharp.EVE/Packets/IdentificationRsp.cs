using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets
{
    public class IdentificationRsp
    {
        private const string TYPE_NAME = "macho.IdentificationRsp";

        /// <summary>
        /// Whether the identification request was accepted or not
        /// </summary>
        public bool Accepted { get; init; }
        /// <summary>
        /// The nodeID of the answering node
        /// </summary>
        public int NodeID { get; init; }

        public static implicit operator PyDataType(IdentificationRsp info)
        {
            return new PyObjectData(TYPE_NAME, new PyTuple(2)
                {
                    [0] = info.Accepted,
                    [1] = info.NodeID
                }
            );
        }

        public static implicit operator IdentificationRsp(PyDataType info)
        {
            if (info is PyObjectData == false)
                throw new InvalidDataException($"Expected container of type ObjectData");

            PyObjectData data = info as PyObjectData;

            if (data.Name != TYPE_NAME)
                throw new InvalidDataException($"Expected ObjectData of type {TYPE_NAME} but got {data.Name}");

            PyTuple arguments = data.Arguments as PyTuple;

            IdentificationRsp result = new IdentificationRsp
            {
                Accepted = arguments[0] as PyBool,
                NodeID = arguments[1] as PyInteger
            };
            
            return result;
        }   
    }
}
