using System.IO;
using EVESharp.Node.Dogma.Exception;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeGT : OpcodeWithBooleanOutput
{
    public Opcode LeftSide  { get; private set; }
    public Opcode RightSide { get; private set; }

    public OpcodeGT (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        // get the right side of the operation
        LeftSide  = Interpreter.Step (reader);
        RightSide = Interpreter.Step (reader);

        if (LeftSide is not OpcodeWithDoubleOutput && LeftSide is not OpcodeWithIntegerOutput)
            throw new DogmaMachineException ("The left side of an GT operand must return a double or integer value");
        if (RightSide is not OpcodeWithDoubleOutput && RightSide is not OpcodeWithIntegerOutput)
            throw new DogmaMachineException ("The right side of an GT operand must return a double or integer value");

        return this;
    }

    public override bool Execute ()
    {
        if (LeftSide is OpcodeWithDoubleOutput left1 && RightSide is OpcodeWithDoubleOutput right1)
            return left1.Execute () > right1.Execute ();
        if (LeftSide is OpcodeWithDoubleOutput left2 && RightSide is OpcodeWithIntegerOutput right2)
            return left2.Execute () > right2.Execute ();
        if (LeftSide is OpcodeWithIntegerOutput left3 && RightSide is OpcodeWithDoubleOutput right3)
            return left3.Execute () > right3.Execute ();
        if (LeftSide is OpcodeWithIntegerOutput left4 && RightSide is OpcodeWithIntegerOutput right4)
            return left4.Execute () > right4.Execute ();

        throw new DogmaMachineException ("Cannot determine what to compare on GT opcode");
    }
}