using System;
using System.IO;
using Node.Dogma.Exception;
using Node.Inventory.Items;
using Node.Inventory.Items.Dogma;

namespace Node.Dogma.Interpreter.Opcodes
{
    /// <summary>
    /// AIM stands for AddItemModifier
    /// </summary>
    public class OpcodeAIM : OpcodeRunnable
    {
        public OpcodeEFF Change { get; private set; }
        public OpcodeDEFATTRIBUTE Attribute { get; private set; }
        
        public OpcodeAIM(Interpreter interpreter) : base(interpreter)
        {
        }

        public override Opcode LoadOpcode(BinaryReader reader)
        {
            Opcode leftSide = this.Interpreter.Step(reader);
            Opcode rightSide = this.Interpreter.Step(reader);
            
            // ensure that both sides can return a value
            if (leftSide is not OpcodeEFF left)
                throw new DogmaMachineException("The left side of a AIM operand must be EFF");
            if (rightSide is not OpcodeDEFATTRIBUTE right)
                throw new DogmaMachineException("The right side of a AIM operand must be DEFATTRIBUTE");
            
            this.Change = left;
            this.Attribute = right;
            
            return this;
        }

        public override void Execute()
        {
            ItemEntity item = this.Change.RightSide.ItemToAffect.GetItem();
            ItemEntity target = this.Interpreter.Environment.Self;

            switch (this.Change.LeftSide.Association)
            {
                case Association.PreAssignment:
                case Association.PostAssignment:
                    item.Attributes[this.Change.RightSide.AttributeToAffect.Attribute] = target.Attributes[this.Attribute.Attribute];
                    break;
                case Association.SkillCheck:
                    throw new DogmaMachineException("SkillCheck not supported yet");
                case Association.PreDiv:
                case Association.PostDiv:
                    if (item.Attributes[this.Change.RightSide.AttributeToAffect.Attribute] != 0)
                        item.Attributes[this.Change.RightSide.AttributeToAffect.Attribute] /= target.Attributes[this.Attribute.Attribute];
                    break;
                case Association.PreMul:
                case Association.PostMul:
                    item.Attributes[this.Change.RightSide.AttributeToAffect.Attribute] *= target.Attributes[this.Attribute.Attribute];
                    break;
                case Association.ModAdd:
                    item.Attributes[this.Change.RightSide.AttributeToAffect.Attribute] += target.Attributes[this.Attribute.Attribute];
                    break;
                case Association.ModSub:
                    item.Attributes[this.Change.RightSide.AttributeToAffect.Attribute] -= target.Attributes[this.Attribute.Attribute];
                    break;
                case Association.PostPercent:
                    break;
                case Association.AddRate:
                case Association.SubRate:
                    throw new DogmaMachineException("AddRate/SubRate not supported yet");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}