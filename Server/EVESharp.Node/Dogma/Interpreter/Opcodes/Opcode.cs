using System.IO;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public abstract class Opcode
{
    public Interpreter Interpreter { get; }

    public Opcode (Interpreter interpreter)
    {
        Interpreter = interpreter;
    }

    public abstract Opcode LoadOpcode (BinaryReader reader);
}