using System.IO;
using EVESharp.Node.Dogma.Exception;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeATT : Opcode
{
    public OpcodeDEFENVIDX    ItemToAffect      { get; private set; }
    public OpcodeDEFATTRIBUTE AttributeToAffect { get; private set; }

    public OpcodeATT (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        Opcode leftSide  = Interpreter.Step (reader);
        Opcode rightSide = Interpreter.Step (reader);

        // ensure that both sides can return a value
        if (leftSide is not OpcodeDEFENVIDX left)
            throw new DogmaMachineException ("The left side of a ATT operand must be a DEFENVIDX");
        if (rightSide is not OpcodeDEFATTRIBUTE right)
            throw new DogmaMachineException ("The right side of a ATT operand must be a DEFATTRIBUTE");

        ItemToAffect      = left;
        AttributeToAffect = right;

        return this;
    }
}