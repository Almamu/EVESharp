using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets
{
    public class IdentificationRsp
    {
        /// <summary>
        /// Whether the identification request was accepted or not
        /// </summary>
        public bool Accepted { get; init; }
        /// <summary>
        /// The nodeID of the answering node
        /// </summary>
        public long NodeID { get; init; }
        /// <summary>
        /// The mode the node is running at
        /// </summary>
        public string Mode { get; init; }

        public static implicit operator PyPacket(IdentificationRsp info)
        {
            return new PyPacket(PyPacket.PacketType.IDENTIFICATION_RSP)
            {
                Source = new PyAddressAny(0),
                Destination = new PyAddressAny(0),
                Payload = new PyTuple(3)
                {
                    [0] = info.Accepted,
                    [1] = info.NodeID,
                    [2] = info.Mode
                }
            };
        }

        public static implicit operator PyDataType(IdentificationRsp info)
        {
            return (PyPacket) info;
        }

        public static implicit operator IdentificationRsp(PyPacket info)
        {
            if (info.Type != PyPacket.PacketType.IDENTIFICATION_RSP)
                throw new InvalidDataException($"Expected packet of IdentificationRsp type");
            
            if (info.Payload.Count != 3)
                throw new InvalidDataException($"Expected tuple with specific element count");

            IdentificationRsp result = new IdentificationRsp
            {
                Accepted = info.Payload[0] as PyBool,
                NodeID = info.Payload[1] as PyInteger,
                Mode = info.Payload[2] as PyString
            };
            
            return result;
        }   
    }
}
