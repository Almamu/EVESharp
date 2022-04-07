using System.IO;
using EVESharp.Node.Dogma.Exception;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

/// <summary>
/// Don't know what EFF stands for, but defines the association type for AIM opcodes
/// </summary>
public class OpcodeEFF : Opcode
{
    public OpcodeDEFASSOCIATION LeftSide  { get; private set; }
    public OpcodeATT            RightSide { get; private set; }
        
    public OpcodeEFF(Interpreter interpreter) : base(interpreter)
    {
    }

    public override Opcode LoadOpcode(BinaryReader reader)
    {
        Opcode leftSide  = this.Interpreter.Step(reader);
        Opcode rightSide = this.Interpreter.Step(reader);
            
        // ensure that both sides can return a value
        if (leftSide is not OpcodeDEFASSOCIATION left)
            throw new DogmaMachineException("The left side of an EFF operand must be a DEFASSOCIATION");
        if (rightSide is not OpcodeATT right)
            throw new DogmaMachineException("The right side of an EFF operand must be an ATT");
            
        this.LeftSide  = left;
        this.RightSide = right;
            
        return this;
    }
}