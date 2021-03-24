using System.IO;
using Node.Inventory.Items.Attributes;

namespace Node.Dogma.Interpreter
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