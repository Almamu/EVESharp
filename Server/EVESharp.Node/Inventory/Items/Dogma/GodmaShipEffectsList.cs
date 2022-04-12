using System.Collections.Generic;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Inventory.Items.Dogma;

public class GodmaShipEffectsList : Dictionary <int, GodmaShipEffect>
{
    public bool TryGetEffect (int effectID, out GodmaShipEffect effect)
    {
        return this.TryGetValue (effectID, out effect);
    }

    public static implicit operator PyDictionary (GodmaShipEffectsList list)
    {
        PyDictionary result = new PyDictionary ();

        foreach ((int effectID, GodmaShipEffect effect) in list)
            if (effect.ShouldStart)
                result [effectID] = effect;

        return result;
    }
}