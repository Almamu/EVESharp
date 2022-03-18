using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets
{
    public class IdentificationReq
    {
        /// <summary>
        /// The address of the requesting node
        /// </summary>
        public string Address { get; init; }
        /// <summary>
        /// The nodeID of the requesting node
        /// </summary>
        public long NodeID { get; init; }
        /// <summary>
        /// The mode the node is running at
        /// </summary>
        public string Mode { get; init; }

        public static implicit operator PyPacket(IdentificationReq info)
        {
            return new PyPacket(PyPacket.PacketType.IDENTIFICATION_REQ)
            {
                Source = new PyAddressAny(0),
                Destination = new PyAddressAny(0),
                Payload = new PyTuple(3)
                {
                    [0] = info.Address,
                    [1] = info.NodeID,
                    [2] = info.Mode
                }
            };
        }

        public static implicit operator PyDataType(IdentificationReq info)
        {
            return (PyPacket) info;
        }

        public static implicit operator IdentificationReq(PyPacket info)
        {
            if (info.Type != PyPacket.PacketType.IDENTIFICATION_REQ)
                throw new InvalidDataException($"Expected packet of IdentificationReq type");

            if (info.Payload.Count != 3)
                throw new InvalidDataException($"Expected tuple with specific element count");
            
            IdentificationReq result = new IdentificationReq
            {
                Address = info.Payload[0] as PyString,
                NodeID = info.Payload[1] as PyInteger,
                Mode = info.Payload[2] as PyString
            };
            
            return result;
        }   
    }
}
