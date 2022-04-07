using System;
using System.IO;
using EVESharp.Node.Dogma.Exception;
using EVESharp.Node.Inventory.Items.Dogma;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeDEFASSOCIATION : Opcode
{
    public Association Association { get; private set; }

    public OpcodeDEFASSOCIATION (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        if (Enum.TryParse (reader.ReadString (), out Association association) == false)
            throw new DogmaMachineException ("Unknown value for DEFASSOCIATION");

        Association = association;

        return this;
    }
}