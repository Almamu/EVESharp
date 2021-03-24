using System.IO;
using Node.Dogma.Exception;

namespace Node.Dogma.Interpreter.Opcodes
{
    public class OpcodeCOMBINE : OpcodeRunnable
    {
        public Opcode LeftSide { get; private set; }
        public Opcode RightSide { get; private set; }
        
        public OpcodeCOMBINE(Interpreter interpreter) : base(interpreter)
        {
        }

        public override Opcode LoadOpcode(BinaryReader reader)
        {
            Opcode leftSide = this.Interpreter.Step(reader);
            Opcode rightSide = this.Interpreter.Step(reader);
            
            // ensure that both sides can return a value
            if (leftSide is not OpcodeRunnable && leftSide is not OpcodeWithBooleanOutput && leftSide is not OpcodeWithDoubleOutput)
                throw new DogmaMachineException("The left side of a COMBINE operand must be some kind of runnable");
            if (rightSide is not OpcodeRunnable && rightSide is not OpcodeWithBooleanOutput && rightSide is not OpcodeWithDoubleOutput)
                throw new DogmaMachineException("The right side of a COMBINE operand must be some kind of runnable");
            
            this.LeftSide = leftSide;
            this.RightSide = rightSide;
            
            return this;
        }

        private void ExecuteSide(Opcode side)
        {
            if (side is OpcodeRunnable runnable)
                runnable.Execute();
            else if (side is OpcodeWithBooleanOutput booleanRunnable)
                booleanRunnable.Execute();
            else if (side is OpcodeWithDoubleOutput doubleRunnable)
                doubleRunnable.Execute();
        }
        
        public override void Execute()
        {
            this.ExecuteSide(this.LeftSide);
            this.ExecuteSide(this.RightSide);
        }
    }
}