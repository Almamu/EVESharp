﻿using System;
using System.IO;
using EVESharp.Node.Dogma.Exception;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.Inventory.Items.Dogma;
using EVESharp.Node.Sessions;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes
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
            Attributes attribute = this.Change.RightSide.AttributeToAffect.Attribute;
            
            // add the modifier to the attribute
            item.Attributes[attribute].AddModifier(this.Change.LeftSide.Association, target.Attributes[this.Attribute.Attribute]);
            
            // notify the character
            this.Interpreter.Environment.ItemFactory.Dogma.NotifyAttributeChange(
                this.Interpreter.Environment.Session.EnsureCharacterIsSelected(),
                attribute,
                item
            );
        }
    }
}