using System.IO;
using EVESharp.Node.Dogma.Exception;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeGTE : OpcodeWithBooleanOutput
{
    public Opcode LeftSide  { get; private set; }
    public Opcode RightSide { get; private set; }
        
    public OpcodeGTE(Interpreter interpreter) : base(interpreter)
    {
    }

    public override Opcode LoadOpcode(BinaryReader reader)
    {
        // get the right side of the operation
        this.LeftSide  = this.Interpreter.Step(reader);
        this.RightSide = this.Interpreter.Step(reader);

        if (this.LeftSide is not OpcodeWithDoubleOutput && this.LeftSide is not OpcodeWithIntegerOutput)
            throw new DogmaMachineException("The left side of an GTE operand must return a double or integer value");
        if (this.RightSide is not OpcodeWithDoubleOutput && this.RightSide is not OpcodeWithIntegerOutput)
            throw new DogmaMachineException("The right side of an GTE operand must return a double or integer value");
            
        return this;
    }

    public override bool Execute()
    {
        if (this.LeftSide is OpcodeWithDoubleOutput left1 && this.RightSide is OpcodeWithDoubleOutput right1)
            return left1.Execute() >= right1.Execute();
        if (this.LeftSide is OpcodeWithDoubleOutput left2 && this.RightSide is OpcodeWithIntegerOutput right2)
            return left2.Execute() >= right2.Execute();
        if (this.LeftSide is OpcodeWithIntegerOutput left3 && this.RightSide is OpcodeWithDoubleOutput right3)
            return left3.Execute() >= right3.Execute();
        if (this.LeftSide is OpcodeWithIntegerOutput left4 && this.RightSide is OpcodeWithIntegerOutput right4)
            return left4.Execute() >= right4.Execute();

        throw new DogmaMachineException("Cannot determine what to compare on GTE opcode");
    }
}