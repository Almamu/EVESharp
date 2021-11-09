using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;

namespace EVESharp.Node.Network
{
    public class CallInformation
    {
        public PyAddress From { get; set; }
        public PyAddress To { get; set; }
        public string Service { get; set; }
        public int CallID { get; set; }
        public PyDictionary NamedPayload { get; set; }
        public Client Client { get; set; }
        public PyPacket.PacketType PacketType { get; set; }
    }
}