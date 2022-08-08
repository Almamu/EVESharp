using System.IO;

namespace EVESharp.EVE.Dogma.Interpreter.Opcodes;

public class OpcodeDEFSTRING : Opcode
{
    public string Definition { get; private set; }

    public OpcodeDEFSTRING (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        this.Definition = reader.ReadString ();

        return this;
    }
}