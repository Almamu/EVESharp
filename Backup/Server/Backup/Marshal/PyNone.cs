using System.IO;

namespace Marshal
{

    public class PyNone : PyObject
    {
        
        public PyNone()
            : base(PyObjectType.None)
        {
            
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(MarshalOpcode.None);
        }
    }

}