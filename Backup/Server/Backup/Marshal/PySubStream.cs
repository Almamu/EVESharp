using System.IO;

namespace Marshal
{

    public class PySubStream : PyObject
    {
        public byte[] RawData { get; set; }
        public PyObject Data { get; set; }
        public Unmarshal DataUnmarshal { get; set; }

        public PySubStream()
            : base(PyObjectType.SubStream)
        {
            
        }

        public PySubStream(byte[] data)
             : base(PyObjectType.SubStream)
        {
            RawData = data;
            DataUnmarshal = new Unmarshal();
            Data = DataUnmarshal.Process(data);
        }

        public PySubStream(PyObject data)
            : base(PyObjectType.SubStream)
        {
            Data = data;
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            uint len = source.ReadSizeEx();
            RawData = source.ReadBytes((int) len);
            DataUnmarshal = new Unmarshal();
            Data = DataUnmarshal.Process(RawData);
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(MarshalOpcode.SubStream);

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);

            binaryWriter.Write(Marshal.Process(Data));
            
            output.WriteSizeEx((uint)memoryStream.Length);
            output.Write(memoryStream.ToArray());
        }

        public override string ToString()
        {
            return "<SubStream: " + Data + ">";
        }

    }

}