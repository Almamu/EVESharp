using System.IO;

namespace Marshal
{
    
    public class PyBool : PyObject
    {
        public bool Value { get; set; }

        public PyBool() : base(PyObjectType.Bool)
        {
        }

        public PyBool(bool val) : this()
        {
            Value = val;
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            switch (op)
            {
                case MarshalOpcode.BoolTrue:
                    Value = true;
                    break;

                case MarshalOpcode.BoolFalse:
                    Value = false;
                    break;
            }
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            switch (Value)
            {
                case true:
                    output.WriteOpcode(MarshalOpcode.BoolTrue);
                    break;

                case false:
                    output.WriteOpcode(MarshalOpcode.BoolFalse);
                    break;
            }
        }

        public override string ToString()
        {
            return "<" + Value + ">";
        }
    }

}