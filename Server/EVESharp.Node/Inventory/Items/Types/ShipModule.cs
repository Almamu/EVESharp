using System;
using System.Linq;
using System.Runtime.InteropServices;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Dogma.Interpreter;
using EVESharp.Node.Dogma.Interpreter.Opcodes;
using EVESharp.Node.Exceptions.dogma;
using EVESharp.Node.Inventory.Items.Dogma;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.StaticData.Dogma;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Inventory.Items.Types
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

            if (this.IsInModuleSlot() == true || this.IsInRigSlot() == true)
            {
                // apply passive effects
                this.ApplyPassiveEffects();
            
                // special case, check for the isOnline attribute and put the module online if so
                if (this.Attributes[StaticData.Inventory.Attributes.isOnline] == 1)
                {
                    this.ApplyEffect("online");
                }
            }
        }
        
        public override PyDictionary GetEffects()
        {
            return this.Effects;
        }

        public void ApplyEffect(string effectName, Client forClient = null)
        {
            // check if the module has the given effect in it's list
            if (this.Type.EffectsByName.TryGetValue(effectName, out Effect effect) == false)
                throw new EffectNotActivatible(this.Type);
            if (this.Effects.TryGetEffect(effect.EffectID, out GodmaShipEffect godmaEffect) == false)
                throw new CustomError("Cannot apply the given effect, our type has it but we dont");
            
            this.ApplyEffect(effect, godmaEffect, forClient);
        }

        private void ApplyEffect(Effect effect, GodmaShipEffect godmaEffect, Client forClient = null)
        {
            if (godmaEffect.ShouldStart == true)
                return;
            
            Ship ship = this.ItemFactory.GetItem<Ship>(this.LocationID);
            Character character = this.ItemFactory.GetItem<Character>(this.OwnerID);

            try
            {
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
            }
            catch (Exception)
            {
                // notify the client about it
                forClient?.NotifyMultiEvent(new OnGodmaShipEffect(godmaEffect));
                throw;
            }

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
            forClient?.NotifyMultiEvent(new OnGodmaShipEffect(godmaEffect));
            
            if (effect.EffectID == (int) EffectsEnum.Online)
                this.ApplyOnlineEffects(forClient);
        }

        public void StopApplyingEffect(string effectName, Client forClient = null)
        {
            // check if the module has the given effect in it's list
            if (this.Type.EffectsByName.TryGetValue(effectName, out Effect effect) == false)
                throw new EffectNotActivatible(this.Type);
            if (this.Effects.TryGetEffect(effect.EffectID, out GodmaShipEffect godmaEffect) == false)
                throw new CustomError("Cannot apply the given effect, our type has it but we dont");

            this.StopApplyingEffect(effect, godmaEffect, forClient);
        }

        private void StopApplyingEffect(Effect effect, GodmaShipEffect godmaEffect, Client forClient = null)
        {
            // ensure the effect is being applied before doing anything
            if (godmaEffect.ShouldStart == false)
                return;
            
            Ship ship = this.ItemFactory.GetItem<Ship>(this.LocationID);
            Character character = this.ItemFactory.GetItem<Character>(this.OwnerID);
            
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
            forClient?.NotifyMultiEvent(new OnGodmaShipEffect(godmaEffect));

            // online effect, this requires some special processing as all the passive effects should also be applied
            if (effect.EffectID == (int) EffectsEnum.Online)
                this.StopApplyingOnlineEffects(forClient);
        }

        private void ApplyEffectsByCategory(EffectCategory category, Client forClient = null)
        {
            foreach ((int _, GodmaShipEffect effect) in this.Effects)
                if (effect.Effect.EffectCategory == category && effect.ShouldStart == false)
                    this.ApplyEffect(effect.Effect, effect, forClient);
        }

        private void StopApplyingEffectsByCategory(EffectCategory category, Client forClient = null)
        {
            foreach ((int _, GodmaShipEffect effect) in this.Effects)
                if (effect.Effect.EffectCategory == category && effect.ShouldStart == true)
                    this.StopApplyingEffect(effect.Effect, effect, forClient);
        }
        
        private void ApplyOnlineEffects(Client forClient = null)
        {
            this.ApplyEffectsByCategory(EffectCategory.Online, forClient);
        }

        private void StopApplyingOnlineEffects(Client forClient = null)
        {
            this.StopApplyingEffectsByCategory(EffectCategory.Online, forClient);
        }

        public void ApplyPassiveEffects(Client forClient = null)
        {
            this.ApplyEffectsByCategory(EffectCategory.Passive, forClient);
        }

        public void StopApplyingPassiveEffects(Client forClient = null)
        {
            this.StopApplyingEffectsByCategory(EffectCategory.Passive, forClient);
        }

        public bool IsHighSlot()
        {
            return this.Effects.ContainsKey((int) EffectsEnum.HighPower) == true;
        }

        public bool IsMediumSlot()
        {
            return this.Effects.ContainsKey((int) EffectsEnum.MedPower) == true;
        }

        public bool IsLowSlot()
        {
            return this.Effects.ContainsKey((int) EffectsEnum.LowPower) == true;
        }

        public bool IsRigSlot()
        {
            return this.Effects.ContainsKey((int) EffectsEnum.RigSlot) == true;
        }
    }
}