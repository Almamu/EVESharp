using System.IO;

namespace Marshal
{

    public class PySubStruct : PyObject
    {
        public PyObject Definition { get; set; }

        public PySubStruct()
            : base(PyObjectType.SubStruct)
        {
            
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            Definition = context.ReadObject(source);
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(MarshalOpcode.SubStruct);
            Definition.Encode(output);
        }
    }

}