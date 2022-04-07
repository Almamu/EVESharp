using System.IO;
using EVESharp.Node.Dogma.Exception;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeAND : OpcodeWithBooleanOutput
{
    public OpcodeWithBooleanOutput FirstCondition  { get; private set; }
    public OpcodeWithBooleanOutput SecondCondition { get; private set; }

    public OpcodeAND (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        Opcode leftSide  = Interpreter.Step (reader);
        Opcode rightSide = Interpreter.Step (reader);

        // ensure that both sides can return a value
        if (leftSide is not OpcodeWithBooleanOutput left)
            throw new DogmaMachineException ("The left side of an AND operand must return a boolean value");
        if (rightSide is not OpcodeWithBooleanOutput right)
            throw new DogmaMachineException ("The right side of an AND operand must return a boolean value");

        FirstCondition  = left;
        SecondCondition = right;

        return this;
    }

    public override bool Execute ()
    {
        return FirstCondition.Execute () && SecondCondition.Execute ();
    }
}