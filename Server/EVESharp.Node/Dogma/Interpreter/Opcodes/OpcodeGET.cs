using System;
using System.IO;
using EVESharp.Node.Dogma.Exception;
using EVESharp.Node.Inventory.Items;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeGET : OpcodeWithDoubleOutput
{
    public OpcodeDEFENVIDX    LeftSide  { get; private set; }
    public OpcodeDEFATTRIBUTE RightSide { get; private set; }
        
    public OpcodeGET(Interpreter interpreter) : base(interpreter)
    {
    }

    public override Opcode LoadOpcode(BinaryReader reader)
    {
        Opcode leftSide  = this.Interpreter.Step(reader);
        Opcode rightSide = this.Interpreter.Step(reader);
            
        // ensure that both sides can return a value
        if (leftSide is not OpcodeDEFENVIDX left)
            throw new DogmaMachineException("The left side of a GET operand must be an environment id");
        if (rightSide is not OpcodeDEFATTRIBUTE right)
            throw new DogmaMachineException("The right side of a GET operand must be an attribute value");

        this.LeftSide  = left;
        this.RightSide = right;
            
        return this;
    }

    public override double Execute()
    {
        return this.LeftSide.GetItem().Attributes[this.RightSide.Attribute];
    }
}