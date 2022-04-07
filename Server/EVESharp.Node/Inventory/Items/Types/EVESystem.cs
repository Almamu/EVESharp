using System;
using EVESharp.Common.Database;
using EVESharp.Node.Inventory.Items.Types.Information;
using EVESharp.Node.StaticData.Inventory;

namespace EVESharp.Node.Inventory.Items.Types;

public class EVESystem : ItemInventory
{
    public EVESystem(Information.Item info) : base(info)
    {
    }

    public EVESystem(ItemEntity @from) : base(@from)
    {
    }

    protected override void LoadContents(Flags ignoreFlags = Flags.None)
    {
        throw new NotSupportedException("EVE System Items cannot load any content in them");
    }
}