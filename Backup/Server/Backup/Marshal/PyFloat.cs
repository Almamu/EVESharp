using System.IO;

namespace Marshal
{

    public class PyFloat : PyObject
    {
        public double Value { get; private set; }

        public PyFloat()
            : base(PyObjectType.Float)
        {
            
        }

        public PyFloat(double value)
            : base(PyObjectType.Float)
        {
            Value = value;
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            if (op == MarshalOpcode.RealZero)
                Value = 0.0d;
            else
                Value = source.ReadDouble();
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            if (Value == 0.0d)
                output.WriteOpcode(MarshalOpcode.RealZero);
            else
            {
                output.WriteOpcode(MarshalOpcode.Real);
                output.Write(Value);
            }
        }

        public override string ToString()
        {
            return "<" + Value + ">";
        }
    }

}