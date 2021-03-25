using System.Collections.Generic;
using Node.Inventory.Items;
using Node.Inventory.Items.Dogma;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Notifications
{
    public class OnGodmaShipEffect : PyMultiEventEntry
    {
        public GodmaShipEffect EffectInfo { get; init; }

        public OnGodmaShipEffect(GodmaShipEffect effectInfo) : base("OnGodmaShipEffect")
        {
            this.EffectInfo = effectInfo;
        }

        protected override List<PyDataType> GetElements()
        {
            return this.EffectInfo;
        }
    }
}