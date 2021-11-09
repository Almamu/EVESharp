using System.IO;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes
{
    public abstract class Opcode
    {
        public Interpreter Interpreter { get; }
        public abstract Opcode LoadOpcode(BinaryReader reader);

        public Opcode(Interpreter interpreter)
        {
            this.Interpreter = interpreter;
        }
    }
}