using System;

namespace EVESharp.EVE.Data.Inventory.Items.Types;

public class EVESystem : ItemInventory
{
    public EVESystem (Database.Inventory.Types.Information.Item info) : base (info) { }

    public EVESystem (ItemEntity from) : base (from) { }

    protected override void LoadContents (Flags ignoreFlags = Flags.None)
    {
        throw new NotSupportedException ("EVE System Items cannot load any content in them");
    }
}