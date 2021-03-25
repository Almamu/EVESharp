using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PythonTypes.Types.Collections;

namespace Node.Inventory.Items.Dogma
{
    public class GodmaShipEffectsList
    {
        private readonly Dictionary<int, GodmaShipEffect> mEffects = new Dictionary<int, GodmaShipEffect>();
        
        public GodmaShipEffect this[int index]
        {
            get => this.mEffects[index];
            set => this.mEffects[index] = value;
        }

        public bool TryGetEffect(int effectID, out GodmaShipEffect effect)
        {
            return this.mEffects.TryGetValue(effectID, out effect);
        }

        public static implicit operator PyDictionary(GodmaShipEffectsList list)
        {
            PyDictionary result = new PyDictionary();

            foreach ((int effectID, GodmaShipEffect effect) in list.mEffects)
                result[effectID] = effect;
            
            return result;
        }
    }
}