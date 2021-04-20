using EVE.Packets.Exceptions;
using Node.Inventory.Items;
using Node.StaticData;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.dogma
{
    public class EffectNotActivatible : UserError
    {
        public EffectNotActivatible(Type type) : base("EffectNotActivatible", new PyDictionary{["moduleName"] = type.Name})
        {
        }
    }
}