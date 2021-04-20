using System;
using System.IO;
using EVE.Packets.Exceptions;
using Node.Dogma.Exception;
using Node.Inventory.Items;

namespace Node.Dogma.Interpreter.Opcodes
{
    public class OpcodeDEFENVIDX : Opcode
    {
        public Inventory.Items.Dogma.Environment Environment { get; private set; }
        
        public OpcodeDEFENVIDX(Interpreter interpreter) : base(interpreter)
        {
        }

        public override Opcode LoadOpcode(BinaryReader reader)
        {
            if (Enum.TryParse(reader.ReadString(), out Inventory.Items.Dogma.Environment environment) == false)
                throw new DogmaMachineException("Cannot determine environment id");

            this.Environment = environment;
            
            return this;
        }

        public ItemEntity GetItem()
        {
            ItemEntity item = null;
            
            switch (this.Environment)
            {
                case Inventory.Items.Dogma.Environment.Self:
                    item = this.Interpreter.Environment.Self;
                    break;
                case Inventory.Items.Dogma.Environment.Char:
                    item = this.Interpreter.Environment.Character;
                    break;
                case Inventory.Items.Dogma.Environment.Ship:
                    item = this.Interpreter.Environment.Ship;
                    break;
                case Inventory.Items.Dogma.Environment.Target:
                case Inventory.Items.Dogma.Environment.Area:
                case Inventory.Items.Dogma.Environment.Other:
                case Inventory.Items.Dogma.Environment.Charge:
                default:
                    throw new CustomError("Unsupported target");
            }

            return item;
        }
    }
}