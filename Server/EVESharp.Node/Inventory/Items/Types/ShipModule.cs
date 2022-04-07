using EVESharp.Node.Dogma;
using EVESharp.Node.Inventory.Items.Dogma;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Inventory.Items.Types;

public class ShipModule : ItemEntity
{
    public GodmaShipEffectsList Effects     { get; }
    public ItemEffects          ItemEffects { get; set; }

    public ShipModule (Information.Item info) : base (info)
    {
        Effects = new GodmaShipEffectsList ();
    }

    public override PyDictionary GetEffects ()
    {
        return Effects;
    }

    public bool IsHighSlot ()
    {
        return Effects.ContainsKey ((int) EffectsEnum.HighPower);
    }

    public bool IsMediumSlot ()
    {
        return Effects.ContainsKey ((int) EffectsEnum.MedPower);
    }

    public bool IsLowSlot ()
    {
        return Effects.ContainsKey ((int) EffectsEnum.LowPower);
    }

    public bool IsRigSlot ()
    {
        return Effects.ContainsKey ((int) EffectsEnum.RigSlot);
    }
}