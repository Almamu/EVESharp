using EVESharp.EVE.Data.Dogma;
using EVESharp.EVE.Data.Inventory.Items.Dogma;
using EVESharp.Node.Dogma;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Data.Inventory.Items.Types;

public class ShipModule : ItemEntity
{
    public GodmaShipEffectsList Effects     { get; }
    public ItemEffects          ItemEffects { get; set; }

    public ShipModule (Information.Item info) : base (info)
    {
        this.Effects = new GodmaShipEffectsList ();
    }

    public override PyDictionary GetEffects ()
    {
        return this.Effects;
    }

    public bool IsHighSlot ()
    {
        return this.Effects.ContainsKey ((int) EffectsEnum.HighPower);
    }

    public bool IsMediumSlot ()
    {
        return this.Effects.ContainsKey ((int) EffectsEnum.MedPower);
    }

    public bool IsLowSlot ()
    {
        return this.Effects.ContainsKey ((int) EffectsEnum.LowPower);
    }

    public bool IsRigSlot ()
    {
        return this.Effects.ContainsKey ((int) EffectsEnum.RigSlot);
    }
}