using System;
using System.IO;
using System.Text;

namespace Marshal
{

    public class PyBuffer : PyObject
    {
        public byte[] Data { get; private set; }

        public PyBuffer()
            : base(PyObjectType.Buffer)
        {
            
        }

        public PyBuffer(byte[] data)
            : base(PyObjectType.Buffer)
        {
            Data = data;
        }

        public PyBuffer(string data)
            : base(PyObjectType.Buffer)
        {
            Data = Encoding.ASCII.GetBytes(data);
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            var size = source.ReadSizeEx();
            Data = source.ReadBytes((int)size);
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(MarshalOpcode.Buffer);
            output.WriteSizeEx(Data.Length);
            output.Write(Data);
        }

        public override string ToString()
        {
            return "<" + BitConverter.ToString(Data) + ">";
        }
    }

}