using System.IO;
using Node.Inventory.Items.Attributes;
using Node.StaticData.Inventory;

namespace Node.Dogma.Interpreter.Opcodes
{
    public class OpcodeDEFATTRIBUTE : Opcode
    {
        public Attributes Attribute { get; private set; }
        
        public OpcodeDEFATTRIBUTE(Interpreter interpreter) : base(interpreter)
        {
        }

        public override Opcode LoadOpcode(BinaryReader reader)
        {
            this.Attribute = (Attributes) reader.ReadInt32();

            return this;
        }
    }
}