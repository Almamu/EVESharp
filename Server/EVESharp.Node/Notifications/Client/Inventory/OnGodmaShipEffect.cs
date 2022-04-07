using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Node.Inventory.Items.Dogma;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Inventory;

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