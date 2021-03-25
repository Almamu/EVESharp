using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PythonTypes.Types.Collections;

namespace Node.Inventory.Items.Dogma
{
    public class GodmaShipEffectsList : Dictionary<int, GodmaShipEffect>
    {
        public bool TryGetEffect(int effectID, out GodmaShipEffect effect)
        {
            return this.TryGetValue(effectID, out effect);
        }

        public static implicit operator PyDictionary(GodmaShipEffectsList list)
        {
            PyDictionary result = new PyDictionary();

            foreach ((int effectID, GodmaShipEffect effect) in list)
                result[effectID] = effect;
            
            return result;
        }
    }
}