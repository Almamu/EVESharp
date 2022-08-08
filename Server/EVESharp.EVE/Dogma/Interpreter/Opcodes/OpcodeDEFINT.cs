using System.IO;

namespace EVESharp.EVE.Dogma.Interpreter.Opcodes;

public class OpcodeDEFINT : OpcodeWithIntegerOutput
{
    public int Value { get; private set; }

    public OpcodeDEFINT (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        this.Value = int.Parse (reader.ReadString ());

        return this;
    }

    public override int Execute ()
    {
        return this.Value;
    }
}