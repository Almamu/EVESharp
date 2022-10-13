using System.IO;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.EVE.Data.Inventory;

namespace EVESharp.EVE.Dogma.Interpreter.Opcodes;

public class OpcodeDEFATTRIBUTE : Opcode
{
    public AttributeTypes Attribute { get; private set; }

    public OpcodeDEFATTRIBUTE (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        this.Attribute = (AttributeTypes) reader.ReadInt32 ();

        return this;
    }
}