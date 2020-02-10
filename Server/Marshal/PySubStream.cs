using System.IO;

namespace Marshal
{

    public class PySubStream : PyObject
    {
        public byte[] RawData { get; set; }
        public PyObject Data { get; set; }
        
        public PySubStream()
            : base(PyObjectType.SubStream)
        {
            Data = null;
            RawData = null;
        }

        public PySubStream(byte[] data)
             : base(PyObjectType.SubStream)
        {
            RawData = data;
            Data = Unmarshal.Process<PyObject>(RawData);
        }

        public PySubStream(PyObject data)
            : base(PyObjectType.SubStream)
        {
            Data = data;
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            uint len = source.ReadSizeEx();

            if (len > 0)
            {
                RawData = source.ReadBytes((int) len);
                Data = Unmarshal.Process<PyObject>(RawData);
            }
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(MarshalOpcode.SubStream);

            // there can be substreams with no data
            if (this.Data == null)
            {
                output.Write((byte) 0);
                return;
            }

            byte[] data = Marshal.Process(Data);
            output.WriteSizeEx((uint)data.Length);
            output.Write(data);
        }

        public override string ToString()
        {
            return "<SubStream: " + Data + ">";
        }

    }

}