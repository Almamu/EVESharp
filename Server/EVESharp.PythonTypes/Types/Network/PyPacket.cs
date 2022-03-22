using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.PythonTypes.Types.Network
{
    /// <summary>
    /// Helper class to work with EVE Online packets
    /// </summary>
    public class PyPacket
    {
        /// <summary>
        /// Type of packets available
        /// </summary>
        public enum PacketType
        {
            // unused
            AUTHENTICATION_REQ = 0,
            // unused
            AUTHENTICATION_RSP = 1,
            IDENTIFICATION_REQ = 2,
            IDENTIFICATION_RSP = 3,
            __Fake_Invalid_Type = 4,
            CALL_REQ = 6,
            CALL_RSP = 7,
            TRANSPORTCLOSED = 8,
            RESOLVE_REQ = 10,
            RESOLVE_RSP = 11,
            NOTIFICATION = 12,
            ERRORRESPONSE = 15,
            SESSIONCHANGENOTIFICATION = 16,
            SESSIONINITIALSTATENOTIFICATION = 18,
            PING_REQ = 20,
            PING_RSP = 21
        }

        /// <summary>
        /// Names of all the packet types, these should always match to properly identify a packet
        /// </summary>
        public static readonly string[] PacketTypeString = 
        {
            "macho.AuthenticationReq",
            "macho.AuthenticationRsp",
            "macho.IdentificationReq",
            "macho.IdentificationRsp",
            "ERROR_TYPE",
            "ERROR_TYPE",
            "macho.CallReq",
            "macho.CallRsp",
            "macho.TransportClosed",
            "ERROR_TYPE",
            "macho.ResolveReq",
            "macho.ResolveRsp",
            "macho.Notification",
            "ERROR_TYPE",
            "ERROR_TYPE",
            "macho.ErrorResponse",
            "macho.SessionChangeNotification",
            "ERROR_TYPE",
            "macho.SessionInitialStateNotification",
            "ERROR_TYPE",
            "macho.PingReq",
            "macho.PingRsp"
        };
        
        /// <summary>
        /// The type of packet
        /// </summary>
        public PacketType Type { get; set; }
        /// <summary>
        /// The address where the packet was originated
        /// </summary>
        public PyAddress Source { get; set; }
        /// <summary>
        /// The address where the packet should arrive
        /// </summary>
        public PyAddress Destination { get; set; }
        /// <summary>
        /// The related userID for the packet
        /// </summary>
        public long UserID { get; set; }
        /// <summary>
        /// Tuple payload with the actual packet data
        /// </summary>
        public PyTuple Payload { get; set; }
        /// <summary>
        /// Out of bounds data with extra information for machoNet or other services
        /// </summary>
        public PyDictionary OutOfBounds { get; set; }
        public string TypeString => PacketTypeString [(int) this.Type];

        protected PyPacket()
        {
            this.Type = PacketType.__Fake_Invalid_Type;
            this.UserID = 0;
            this.Payload = null;
            this.OutOfBounds = null;
            this.Source = null;
            this.Destination = null;
        }

        /// <summary>
        /// Creates a new PyPacket with default values for the given packet type
        /// </summary>
        /// <param name="type">The type of the packet to create</param>
        public PyPacket(PacketType type) : this()
        {
            this.Type = type;
        }

        public static implicit operator PyDataType(PyPacket packet)
        {
            PyTuple args = new PyTuple(6)
            {
                [0] = (int) packet.Type,
                [1] = packet.Source,
                [2] = packet.Destination,
                [3] = (packet.UserID == 0) ? null : packet.UserID,
                [4] = packet.Payload,
                [5] = packet.OutOfBounds
            };
            
            return new PyObjectData(packet.TypeString, args);
        }

        /// <summary>
        /// Checks if the given PyDataType looks like a PyPacket
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsPyPacket(PyDataType data)
        {
            if (data is PyChecksumedStream stream)
                data = stream.Data;

            if (data is PySubStream subStream)
                data = subStream.Stream;

            return data is PyObjectData;
        }

        public static implicit operator PyPacket(PyDataType data)
        {
            PyPacket result = new PyPacket();

            // packet can be wrapped in ChecksumedStreams and SubStreams, so unwind these first
            // this should leave a PyObjectData accessible, which should be the actual packet's content
            if (data is PyChecksumedStream stream)
                data = stream.Data;

            if (data is PySubStream subStream)
                data = subStream.Stream;

            if (data is PyObjectData == false)
                throw new InvalidDataException($"Expected container of type PyObjectData for PyPacket, but got {data}");

            PyObjectData objectData = data as PyObjectData;
            PyTuple packetData = objectData.Arguments as PyTuple;

            if (packetData is null || packetData.Count != 6)
                throw new InvalidDataException();

            result.Type = (PacketType) (int) (packetData[0] as PyInteger);
            result.Source = (PyAddress) packetData[1];
            result.Destination = (PyAddress) packetData[2];
            result.UserID = packetData[3] as PyInteger ?? 0;
            result.Payload = packetData[4] as PyTuple;
            result.OutOfBounds = packetData[5] as PyDictionary;

            // ensure consistency between the integer type and the string type indicators
            if(result.TypeString != objectData.Name)
                throw new InvalidDataException($"Received a packet of type {result.Type} with an unexpected name {objectData.Name}");
            
            return result;
        }
    }
}