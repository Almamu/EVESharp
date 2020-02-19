using System.IO;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    public class PyPacket
    {
        public PyPacket()
        {
            Type = MachoMessageType.__Fake_Invalid_Type;
            type_string = "none";
            UserID = 0;
            Payload = null;
            NamedPayload = null;
            Source = null;
            Destination = null;
        }

        public static implicit operator PyDataType(PyPacket packet)
        {
            PyTuple args = new PyTuple(6);

            args[0] = (int) packet.Type;
            args[1] = packet.Source;
            args[2] = packet.Destination;
            args[3] = (packet.UserID == 0) ? (PyDataType) new PyNone() : packet.UserID;
            args[4] = packet.Payload;
            args[5] = packet.NamedPayload;

            return new PyObjectData(packet.type_string, args);
        }

        public static implicit operator PyPacket(PyDataType data)
        {
            PyPacket result = new PyPacket();

            if (data is PyChecksumedStream)
                data = (data as PyChecksumedStream).Data;

            if (data is PySubStream)
                data = (data as PySubStream).Stream;

            if (data is PyObjectData == false)
                throw new InvalidDataException();

            PyObjectData objectData = data as PyObjectData;

            result.type_string = objectData.Name;

            PyTuple packetData = objectData.Arguments as PyTuple;

            if (packetData.Count != 6)
                throw new InvalidDataException();

            result.Type = (MachoMessageType) (int) (packetData[0] as PyInteger);
            result.Source = (PyAddress) packetData[1];
            result.Destination = (PyAddress) packetData[2];
            result.UserID = (packetData[3] is PyNone) ? 0 : (long) (packetData[3] as PyInteger);
            result.Payload = packetData[4] as PyTuple;
            result.NamedPayload = packetData[5] as PyDictionary;

            return result;
        }

        public MachoMessageType Type;
        public PyAddress Source;
        public PyAddress Destination;
        public long UserID;
        public PyTuple Payload;
        public PyDictionary NamedPayload;
        public string type_string;
    }
}