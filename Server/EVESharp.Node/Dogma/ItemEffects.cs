using System;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Dogma.Interpreter.Opcodes;
using EVESharp.Node.Exceptions.dogma;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Dogma;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.Sessions;
using EVESharp.Node.StaticData.Dogma;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Dogma;

public class ItemEffects
{
    /// <summary>
    /// The item to apply effects to
    /// </summary>
    public ShipModule Module { get; }
    /// <summary>
    /// The item factory to handle the item
    /// </summary>
    private ItemFactory ItemFactory { get; }
    
    public ItemEffects(ShipModule item, ItemFactory itemFactory)
    {
        this.Module = item;
        this.ItemFactory = itemFactory;
        
        foreach ((int effectID, Effect effect) in this.Module.Type.Effects)
        {
            // create effects entry in the list
            this.Module.Effects[effectID] = new GodmaShipEffect()
            {
                AffectedItem = this.Module,
                Effect = effect,
                ShouldStart = false,
                StartTime = 0,
                Duration = 0,
            };
        }

        if (this.Module.IsInModuleSlot() == true || this.Module.IsInRigSlot() == true)
        {
            // apply passive effects
            this.ApplyPassiveEffects();
            
            // special case, check for the isOnline attribute and put the module online if so
            if (this.Module.Attributes[StaticData.Inventory.Attributes.isOnline] == 1)
            {
                this.ApplyEffect("online");
            }
        }
    }

    public void ApplyEffect(string effectName, Session session = null)
    {
        // check if the module has the given effect in it's list
        if (this.Module.Type.EffectsByName.TryGetValue(effectName, out Effect effect) == false)
            throw new EffectNotActivatible(this.Module.Type);
        if (this.Module.Effects.TryGetEffect(effect.EffectID, out GodmaShipEffect godmaEffect) == false)
            throw new CustomError("Cannot apply the given effect, our type has it but we dont");
        
        this.ApplyEffect(effect, godmaEffect, session);
    }

    private void ApplyEffect(Effect effect, GodmaShipEffect godmaEffect, Session session = null)
    {
        if (godmaEffect.ShouldStart == true)
            return;
        
        Ship ship = this.ItemFactory.GetItem<Ship>(this.Module.LocationID);
        Character character = this.ItemFactory.GetItem<Character>(this.Module.OwnerID);

        try
        {
            // create the environment for this run
            Node.Dogma.Interpreter.Environment env = new Node.Dogma.Interpreter.Environment()
            {
                Character = character,
                Self = this.Module,
                Ship = ship,
                Target = null,
                Session = session,
                ItemFactory = this.ItemFactory
            };
            
            Opcode opcode = new Interpreter.Interpreter(env).Run(effect.PreExpression.VMCode);
        
            if (opcode is OpcodeRunnable runnable)
                runnable.Execute();
            else if (opcode is OpcodeWithBooleanOutput booleanOutput)
                booleanOutput.Execute();
            else if (opcode is OpcodeWithDoubleOutput doubleOutput)
                doubleOutput.Execute();
        }
        catch (System.Exception)
        {
            // notify the client about it
            // TODO: THIS MIGHT NEED MORE NOTIFICATIONS
            this.ItemFactory.Dogma.QueueMultiEvent(session.EnsureCharacterIsSelected(), new OnGodmaShipEffect(godmaEffect));
            throw;
        }

        // ensure the module is saved
        this.Module.Persist();

        PyDataType duration = 0;

        if (effect.DurationAttributeID is not null)
            duration = this.Module.Attributes[(int) effect.DurationAttributeID];

        // update things like duration, start, etc
        godmaEffect.StartTime = DateTime.UtcNow.ToFileTimeUtc();
        godmaEffect.ShouldStart = true;
        godmaEffect.Duration = duration;
        
        // notify the client about it
        // TODO: THIS MIGHT NEED MORE NOTIFICATIONS
        // TODO: CHECK IF THIS MULTIEVENT IS RIGHT OR NOT
        this.ItemFactory.Dogma.QueueMultiEvent(this.Module.OwnerID, new OnGodmaShipEffect(godmaEffect));

        if (effect.EffectID == (int) EffectsEnum.Online)
            this.ApplyOnlineEffects(session);
    }

    public void StopApplyingEffect(string effectName, Session session = null)
    {
        // check if the module has the given effect in it's list
        if (this.Module.Type.EffectsByName.TryGetValue(effectName, out Effect effect) == false)
            throw new EffectNotActivatible(this.Module.Type);
        if (this.Module.Effects.TryGetEffect(effect.EffectID, out GodmaShipEffect godmaEffect) == false)
            throw new CustomError("Cannot apply the given effect, our type has it but we dont");

        this.StopApplyingEffect(effect, godmaEffect, session);
    }

    private void StopApplyingEffect(Effect effect, GodmaShipEffect godmaEffect, Session session = null)
    {
        // ensure the effect is being applied before doing anything
        if (godmaEffect.ShouldStart == false)
            return;
        
        Ship ship = this.ItemFactory.GetItem<Ship>(this.Module.LocationID);
        Character character = this.ItemFactory.GetItem<Character>(this.Module.OwnerID);
        
        // create the environment for this run
        Node.Dogma.Interpreter.Environment env = new Node.Dogma.Interpreter.Environment()
        {
            Character = character,
            Self = this.Module,
            Ship = ship,
            Target = null,
            Session = session,
            ItemFactory = this.ItemFactory
        };

        Opcode opcode = new Interpreter.Interpreter(env).Run(effect.PostExpression.VMCode);
        
        if (opcode is OpcodeRunnable runnable)
            runnable.Execute();
        else if (opcode is OpcodeWithBooleanOutput booleanOutput)
            booleanOutput.Execute();
        else if (opcode is OpcodeWithDoubleOutput doubleOutput)
            doubleOutput.Execute();

        // ensure the module is saved
        this.Module.Persist();
            
        // update things like duration, start, etc
        godmaEffect.StartTime = 0;
        godmaEffect.ShouldStart = false;
        godmaEffect.Duration = 0;
        
        // notify the client about it
        // TODO: THIS MIGHT NEED MORE NOTIFICATIONS
        this.ItemFactory.Dogma.QueueMultiEvent(session.EnsureCharacterIsSelected(), new OnGodmaShipEffect(godmaEffect));

        // online effect, this requires some special processing as all the passive effects should also be applied
        if (effect.EffectID == (int) EffectsEnum.Online)
            this.StopApplyingOnlineEffects(session);
    }

    private void ApplyEffectsByCategory(EffectCategory category, Session session = null)
    {
        foreach ((int _, GodmaShipEffect effect) in this.Module.Effects)
            if (effect.Effect.EffectCategory == category && effect.ShouldStart == false)
                this.ApplyEffect(effect.Effect, effect, session);
    }

    private void StopApplyingEffectsByCategory(EffectCategory category, Session session = null)
    {
        foreach ((int _, GodmaShipEffect effect) in this.Module.Effects)
            if (effect.Effect.EffectCategory == category && effect.ShouldStart == true)
                this.StopApplyingEffect(effect.Effect, effect, session);
    }
    
    private void ApplyOnlineEffects(Session session = null)
    {
        this.ApplyEffectsByCategory(EffectCategory.Online, session);
    }

    private void StopApplyingOnlineEffects(Session session = null)
    {
        this.StopApplyingEffectsByCategory(EffectCategory.Online, session);
    }

    public void ApplyPassiveEffects(Session session = null)
    {
        this.ApplyEffectsByCategory(EffectCategory.Passive, session);
    }

    public void StopApplyingPassiveEffects(Session session = null)
    {
        this.StopApplyingEffectsByCategory(EffectCategory.Passive, session);
    }
}