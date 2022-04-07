using System.IO;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeDEFSTRING : Opcode
{
    public string Definition { get; private set; }
        
    public override Opcode LoadOpcode(BinaryReader reader)
    {
        this.Definition = reader.ReadString();

        return this;
    }

    public OpcodeDEFSTRING(Interpreter interpreter) : base(interpreter)
    {
    }
}