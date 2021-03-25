using System;
using System.Runtime.InteropServices;
using Node.Dogma.Interpreter;
using Node.Dogma.Interpreter.Opcodes;
using Node.Exceptions.dogma;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Dogma;
using Node.Inventory.Notifications;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items.Types
{
    public class ShipModule : ItemEntity
    {
        public GodmaShipEffectsList Effects { get; }
        
        public ShipModule(ItemEntity @from) : base(@from)
        {
            this.Effects = new GodmaShipEffectsList();

            foreach ((int effectID, Effect effect) in this.Type.Effects)
            {
                // create effects entry in the list
                this.Effects[effectID] = new GodmaShipEffect()
                {
                    AffectedItem = this,
                    Effect = effect,
                    ShouldStart = false,
                    StartTime = 0,
                    Duration = 0,
                };
            }
            
            // special case, check for the isOnline attribute and put the module online if so
            if (this.Attributes[AttributeEnum.isOnline] == 1)
            {
                GodmaShipEffect effect = this.Effects[16];

                effect.ShouldStart = true;
                effect.StartTime = DateTime.UtcNow.ToFileTimeUtc();
            }
        }
        
        public override PyDictionary GetEffects()
        {
            return this.Effects;
        }

        public void ApplyEffect(string effectName, Client forClient)
        {
            Ship ship = this.ItemFactory.ItemManager.GetItem<Ship>((int) forClient.ShipID);
            Character character = this.ItemFactory.ItemManager.GetItem<Character>(forClient.EnsureCharacterIsSelected());
            
            // check if the module has the given effect in it's list
            if (this.Type.EffectsByName.TryGetValue(effectName, out Effect effect) == false)
                throw new EffectNotActivatible(this.Type);
            if (this.Effects.TryGetEffect(effect.EffectID, out GodmaShipEffect godmaEffect) == false)
                throw new CustomError("Cannot apply the given effect, our type has it but we dont");
            if (godmaEffect.ShouldStart == true)
                return;
            
            // create the environment for this run
            Node.Dogma.Interpreter.Environment env = new Node.Dogma.Interpreter.Environment()
            {
                Character = character,
                Self = this,
                Ship = ship,
                Target = null,
                Client = forClient
            };

            Opcode opcode = new Interpreter(env).Run(effect.PreExpression.VMCode);
            
            if (opcode is OpcodeRunnable runnable)
                runnable.Execute();
            else if (opcode is OpcodeWithBooleanOutput booleanOutput)
                booleanOutput.Execute();
            else if (opcode is OpcodeWithDoubleOutput doubleOutput)
                doubleOutput.Execute();

            // ensure the module is saved
            this.Persist();

            PyDataType duration = 0;

            if (effect.DurationAttributeID is not null)
                duration = this.Attributes[(int) effect.DurationAttributeID];

            // update things like duration, start, etc
            godmaEffect.StartTime = DateTime.UtcNow.ToFileTimeUtc();
            godmaEffect.ShouldStart = true;
            godmaEffect.Duration = duration;
            
            // notify the client about it
            forClient.NotifyMultiEvent(new OnGodmaShipEffect(godmaEffect));
        }

        public void StopApplyingEffect(string effectName, Client forClient)
        {
            Ship ship = this.ItemFactory.ItemManager.GetItem<Ship>((int) forClient.ShipID);
            Character character = this.ItemFactory.ItemManager.GetItem<Character>(forClient.EnsureCharacterIsSelected());
            
            // check if the module has the given effect in it's list
            if (this.Type.EffectsByName.TryGetValue(effectName, out Effect effect) == false)
                throw new EffectNotActivatible(this.Type);
            if (this.Effects.TryGetEffect(effect.EffectID, out GodmaShipEffect godmaEffect) == false)
                throw new CustomError("Cannot apply the given effect, our type has it but we dont");

            // ensure the effect is being applied before doing anything
            if (godmaEffect.ShouldStart == false)
                return;
            
            // create the environment for this run
            Node.Dogma.Interpreter.Environment env = new Node.Dogma.Interpreter.Environment()
            {
                Character = character,
                Self = this,
                Ship = ship,
                Target = null,
                Client = forClient
            };

            Opcode opcode = new Interpreter(env).Run(effect.PostExpression.VMCode);
            
            if (opcode is OpcodeRunnable runnable)
                runnable.Execute();
            else if (opcode is OpcodeWithBooleanOutput booleanOutput)
                booleanOutput.Execute();
            else if (opcode is OpcodeWithDoubleOutput doubleOutput)
                doubleOutput.Execute();

            // ensure the module is saved
            this.Persist();
                
            // update things like duration, start, etc
            godmaEffect.StartTime = 0;
            godmaEffect.ShouldStart = false;
            godmaEffect.Duration = 0;
            
            // notify the client about it
            forClient.NotifyMultiEvent(new OnGodmaShipEffect(godmaEffect));
        }
    }
}