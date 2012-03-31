using System.IO;

namespace Marshal
{

    public class PyLongLong : PyObject
    {
        public long Value { get; private set; }

        public PyLongLong()
            : base(PyObjectType.LongLong)
        {
            
        }

        public PyLongLong(long val)
            : base(PyObjectType.LongLong)
        {
            Value = val;
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            Value = source.ReadInt64();
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(MarshalOpcode.IntegerLongLong);
            output.Write(Value);
        }

        public override string ToString()
        {
            return "<" + Value + ">";
        }
    }

}