using System.IO;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Inventory.Items.Attributes;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeDEFATTRIBUTE : Opcode
{
    public Attributes Attribute { get; private set; }
        
    public OpcodeDEFATTRIBUTE(Interpreter interpreter) : base(interpreter)
    {
    }

    public override Opcode LoadOpcode(BinaryReader reader)
    {
        this.Attribute = (Attributes) reader.ReadInt32();

        return this;
    }
}