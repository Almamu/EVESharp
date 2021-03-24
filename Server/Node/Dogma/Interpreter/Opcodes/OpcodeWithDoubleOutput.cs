namespace Node.Dogma.Interpreter.Opcodes
{
    public abstract class OpcodeWithDoubleOutput : Opcode
    {
        protected OpcodeWithDoubleOutput(Interpreter interpreter) : base(interpreter)
        {
        }

        public abstract double Execute();
    }
}