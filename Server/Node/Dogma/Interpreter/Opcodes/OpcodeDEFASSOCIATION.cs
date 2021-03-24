using System;
using System.IO;
using Node.Dogma.Exception;
using Node.Inventory.Items.Dogma;

namespace Node.Dogma.Interpreter.Opcodes
{
    public class OpcodeDEFASSOCIATION : Opcode
    {
        public Association Association { get; private set; }
        
        public OpcodeDEFASSOCIATION(Interpreter interpreter) : base(interpreter)
        {
        }

        public override Opcode LoadOpcode(BinaryReader reader)
        {
            if (Enum.TryParse(reader.ReadString(), out Association association) == false)
                throw new DogmaMachineException("Unknown value for DEFASSOCIATION");

            this.Association = association;
            
            return this;
        }
    }
}