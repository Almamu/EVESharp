using System.IO;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeDEFSTRING : Opcode
{
    public string Definition { get; private set; }

    public OpcodeDEFSTRING (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        Definition = reader.ReadString ();

        return this;
    }
}