using System.IO;
using EVESharp.Node.Dogma.Exception;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeOR : OpcodeWithBooleanOutput
{
    public OpcodeWithBooleanOutput LeftSide  { get; private set; }
    public OpcodeWithBooleanOutput RightSide { get; private set; }

    public OpcodeOR(Interpreter interpreter) : base(interpreter)
    {
    }

    public override Opcode LoadOpcode(BinaryReader reader)
    {
        Opcode leftSide  = this.Interpreter.Step(reader);
        Opcode rightSide = this.Interpreter.Step(reader);
            
        // ensure that both sides can return a value
        if (leftSide is not OpcodeWithBooleanOutput left)
            throw new DogmaMachineException("The left side of an OR operand must return a boolean value");
        if (rightSide is not OpcodeWithBooleanOutput right)
            throw new DogmaMachineException("The right side of an OR operand must return a boolean value");

        this.LeftSide  = left;
        this.RightSide = right;
            
        return this;
    }

    public override bool Execute()
    {
        return this.LeftSide.Execute() || this.RightSide.Execute();
    }
}