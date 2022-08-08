using System.IO;
using EVESharp.EVE.Data.Inventory;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeDEFATTRIBUTE : Opcode
{
    public AttributeTypes Attribute { get; private set; }

    public OpcodeDEFATTRIBUTE (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        Attribute = (AttributeTypes) reader.ReadInt32 ();

        return this;
    }
}