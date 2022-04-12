using System.IO;
using EVESharp.EVE.Dogma.Exception;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeGET : OpcodeWithDoubleOutput
{
    public OpcodeDEFENVIDX    LeftSide  { get; private set; }
    public OpcodeDEFATTRIBUTE RightSide { get; private set; }

    public OpcodeGET (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        Opcode leftSide  = Interpreter.Step (reader);
        Opcode rightSide = Interpreter.Step (reader);

        // ensure that both sides can return a value
        if (leftSide is not OpcodeDEFENVIDX left)
            throw new DogmaMachineException ("The left side of a GET operand must be an environment id");
        if (rightSide is not OpcodeDEFATTRIBUTE right)
            throw new DogmaMachineException ("The right side of a GET operand must be an attribute value");

        LeftSide  = left;
        RightSide = right;

        return this;
    }

    public override double Execute ()
    {
        return LeftSide.GetItem ().Attributes [RightSide.Attribute];
    }
}