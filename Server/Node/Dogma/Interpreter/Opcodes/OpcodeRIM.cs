using System;
using System.IO;
using Node.Dogma.Exception;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Dogma;

namespace Node.Dogma.Interpreter.Opcodes
{
    /// <summary>
    /// RIM stands for RemoveItemModifier
    /// </summary>
    public class OpcodeRIM : OpcodeRunnable
    {
        public OpcodeEFF Change { get; private set; }
        public OpcodeDEFATTRIBUTE Attribute { get; private set; }
        
        public OpcodeRIM(Interpreter interpreter) : base(interpreter)
        {
        }

        public override Opcode LoadOpcode(BinaryReader reader)
        {
            Opcode leftSide = this.Interpreter.Step(reader);
            Opcode rightSide = this.Interpreter.Step(reader);
            
            // ensure that both sides can return a value
            if (leftSide is not OpcodeEFF left)
                throw new DogmaMachineException("The left side of a RIM operand must be EFF");
            if (rightSide is not OpcodeDEFATTRIBUTE right)
                throw new DogmaMachineException("The right side of a RIM operand must be DEFATTRIBUTE");
            
            this.Change = left;
            this.Attribute = right;
            
            return this;
        }

        public override void Execute()
        {
            ItemEntity item = this.Change.RightSide.ItemToAffect.GetItem();
            ItemEntity target = this.Interpreter.Environment.Self;
            AttributeEnum attribute = this.Change.RightSide.AttributeToAffect.Attribute;
            
            // add the modifier to the attribute
            item.Attributes[attribute].RemoveModifier(this.Change.LeftSide.Association, target.Attributes[this.Attribute.Attribute]);
            
            // notify the character
            this.Interpreter.Environment.Client?.NotifyAttributeChange(attribute, item);
        }
    }
}