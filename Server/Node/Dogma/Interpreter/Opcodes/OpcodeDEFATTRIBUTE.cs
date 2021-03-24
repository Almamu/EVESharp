using System.IO;
using Node.Inventory.Items.Attributes;

namespace Node.Dogma.Interpreter.Opcodes
{
    public class OpcodeDEFATTRIBUTE : Opcode
    {
        public AttributeEnum Attribute { get; private set; }
        
        public OpcodeDEFATTRIBUTE(Interpreter interpreter) : base(interpreter)
        {
        }

        public override Opcode LoadOpcode(BinaryReader reader)
        {
            this.Attribute = (AttributeEnum) reader.ReadInt32();

            return this;
        }
    }
}