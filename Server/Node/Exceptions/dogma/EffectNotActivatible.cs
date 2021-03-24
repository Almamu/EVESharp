using Node.Inventory.Items;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.dogma
{
    public class EffectNotActivatible : UserError
    {
        public EffectNotActivatible(ItemType type) : base("EffectNotActivatible", new PyDictionary{["moduleName"] = type.Name})
        {
        }
    }
}