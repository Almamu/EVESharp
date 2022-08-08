using System.IO;
using EVESharp.EVE.Dogma.Exception;

namespace EVESharp.EVE.Dogma.Interpreter.Opcodes;

public class OpcodeIF : OpcodeWithBooleanOutput
{
    public OpcodeWithBooleanOutput Condition   { get; private set; }
    public Opcode                  RunWhenTrue { get; private set; }

    public OpcodeIF (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        Opcode leftSide  = this.Interpreter.Step (reader);
        Opcode rightSide = this.Interpreter.Step (reader);

        // ensure that both sides can return a value
        if (leftSide is not OpcodeWithBooleanOutput left)
            throw new DogmaMachineException ("The left side of an IF operand must return a boolean value");

        if (rightSide is not OpcodeRunnable && rightSide is not OpcodeWithBooleanOutput && rightSide is not OpcodeWithDoubleOutput)
            throw new DogmaMachineException ("The right side of an IF operand must be some kind of runnable");

        this.Condition   = left;
        this.RunWhenTrue = rightSide;

        return this;
    }

    public override bool Execute ()
    {
        if (this.Condition.Execute ())
        {
            // run the right side
            if (this.RunWhenTrue is OpcodeRunnable runnable)
                runnable.Execute ();
            else if (this.RunWhenTrue is OpcodeWithBooleanOutput booleanRunnable)
                booleanRunnable.Execute ();
            else if (this.RunWhenTrue is OpcodeWithDoubleOutput doubleRunnable)
                doubleRunnable.Execute ();

            return true;
        }

        return false;
    }
}