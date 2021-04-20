using System.Collections.Generic;
using EVE.Packets.Complex;
using Node.Inventory.Items.Dogma;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Inventory
{
    public class OnGodmaShipEffect : ClientNotification
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