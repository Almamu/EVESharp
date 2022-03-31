using System.IO;
using EVESharp.Node.Dogma.Exception;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.Sessions;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes
{
    public class OpcodeSET : OpcodeRunnable
    {
        public Opcode LeftSide { get; private set; }
        public Opcode Value { get; private set; }
        
        public OpcodeSET(Interpreter interpreter) : base(interpreter)
        {
        }

        public override Opcode LoadOpcode(BinaryReader reader)
        {
            this.LeftSide = this.Interpreter.Step(reader);
            this.Value = this.Interpreter.Step(reader);

            return this;
        }

        public override void Execute()
        {
            if (this.LeftSide is OpcodeATT att)
            {
                if (this.Value is OpcodeDEFINT defint)
                {
                    ItemEntity item = att.ItemToAffect.GetItem();
                    Attributes attribute = att.AttributeToAffect.Attribute;
                    
                    item.Attributes[att.AttributeToAffect.Attribute].Integer = defint.Value;
                    
                    // notify the character
                    this.Interpreter.Environment.ItemFactory.Dogma.NotifyAttributeChange(
                        this.Interpreter.Environment.Session.EnsureCharacterIsSelected(),
                        attribute,
                        item
                    );
                }
            }
            else
                throw new DogmaMachineException("Unexpected parameter for left side of SET opcode");
        }
    }
}