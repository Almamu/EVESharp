namespace Node.Dogma.Interpreter.Opcodes
{
    public abstract class OpcodeWithIntegerOutput : Opcode
    {
        protected OpcodeWithIntegerOutput(Interpreter interpreter) : base(interpreter)
        {
        }

        public abstract int Execute();
    }
}