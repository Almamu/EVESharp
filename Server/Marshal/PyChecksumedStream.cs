using System.IO;

namespace Marshal
{

    public class PyChecksumedStream : PyObject
    {
        public uint Checksum { get; private set; }
        public PyObject Data { get; private set; }

        public PyChecksumedStream(PyObject data)
            : base(PyObjectType.ChecksumedStream)
        {
            Data = data;
        }

        public PyChecksumedStream()
            : base(PyObjectType.ChecksumedStream)
        {
            
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            Checksum = source.ReadUInt32();
            Data = context.ReadObject(source);
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(MarshalOpcode.ChecksumedStream);
            var ms = new MemoryStream();
            var tmp = new BinaryWriter(ms);
            Data.Encode(tmp);
            var data = ms.ToArray();
            Checksum = Adler32.Checksum(data);
            output.Write(Checksum);
            output.Write(data);
        }
    }

}