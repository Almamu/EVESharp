using System.IO;

namespace EVESharp.EVE.Dogma.Interpreter.Opcodes;

public abstract class Opcode
{
    public Interpreter Interpreter { get; }

    public Opcode (Interpreter interpreter)
    {
        this.Interpreter = interpreter;
    }

    public abstract Opcode LoadOpcode (BinaryReader reader);
}