using System.Collections.Generic;
using Node.Inventory.Items.Dogma;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Inventory
{
    public class OnGodmaShipEffect : PyNotification
    {
        public GodmaShipEffect EffectInfo { get; init; }

        public OnGodmaShipEffect(GodmaShipEffect effectInfo) : base("OnGodmaShipEffect")
        {
            this.EffectInfo = effectInfo;
        }

        public override List<PyDataType> GetElements()
        {
            return this.EffectInfo;
        }
    }
}