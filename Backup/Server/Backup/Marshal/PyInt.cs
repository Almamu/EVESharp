using System.IO;

namespace Marshal
{

    public class PyInt : PyObject
    {
        public int Value { get; private set; }

        public PyInt()
            : base(PyObjectType.Long)
        {
            
        }

        public PyInt(int val)
            : base(PyObjectType.Long)
        {
            Value = val;
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            if (op == MarshalOpcode.IntegerOne)
                Value = 1;
            else if (op == MarshalOpcode.IntegerZero)
                Value = 0;
            else if (op == MarshalOpcode.IntegerMinusOne)
                Value = -1;
            else if (op == MarshalOpcode.IntegerByte)
                Value = source.ReadByte();
            else if (op == MarshalOpcode.IntegerSignedShort)
                Value = source.ReadInt16();
            else if (op == MarshalOpcode.IntegerLong)
                Value = source.ReadInt32();
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            if (Value == 1)
                output.WriteOpcode(MarshalOpcode.IntegerOne);
            else if (Value == 0)
                output.WriteOpcode(MarshalOpcode.IntegerZero);
            else if (Value == -1)
                output.WriteOpcode(MarshalOpcode.IntegerMinusOne);
            else if (Value < 127)
            {
                output.WriteOpcode(MarshalOpcode.IntegerByte);
                output.Write((byte)Value);
            }
            else if (Value < 32768)
            {
                output.WriteOpcode(MarshalOpcode.IntegerSignedShort);
                output.Write((short)Value);
            }
            else
            {
                output.WriteOpcode(MarshalOpcode.IntegerLong);
                output.Write(Value);
            }
        }

        public override string ToString()
        {
            return "<" + Value + ">";
        }
    }

}