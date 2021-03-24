using System.IO;

namespace Node.Dogma.Interpreter.Opcodes
{
    public class OpcodeDEFINT : Opcode
    {
        public int Value { get; private set; }
        
        public override Opcode LoadOpcode(BinaryReader reader)
        {
            this.Value = int.Parse(reader.ReadString());

            return this;
        }

        public OpcodeDEFINT(Interpreter interpreter) : base(interpreter)
        {
        }
    }
}