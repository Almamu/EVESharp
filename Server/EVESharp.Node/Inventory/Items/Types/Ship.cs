using System.Collections.Generic;
using System.Linq;
using EVESharp.EVE.Data.Inventory;

namespace EVESharp.Node.Inventory.Items.Types;

public class Ship : ItemInventory
{
    public Dictionary <Flags, ItemEntity> ActiveModules =>
        Items
            .Where (x => x.Value.Flag.IsHighModule () || x.Value.Flag.IsMediumModule () || x.Value.Flag.IsLowModule ())
            .ToDictionary (x => x.Value.Flag, x => x.Value);

    public Dictionary <Flags, ItemEntity> RigSlots =>
        Items
            .Where (x => x.Value.Flag.IsRigModule ())
            .ToDictionary (x => x.Value.Flag, x => x.Value);

    public Dictionary <Flags, ItemEntity> HighSlotModules =>
        Items
            .Where (x => x.Value.Flag.IsHighModule ())
            .ToDictionary (x => x.Value.Flag, x => x.Value);

    public Dictionary <Flags, ItemEntity> MediumSlotModules =>
        Items
            .Where (x => x.Value.Flag.IsMediumModule ())
            .ToDictionary (x => x.Value.Flag, x => x.Value);

    public Dictionary <Flags, ItemEntity> LowSlotModules =>
        Items
            .Where (x => x.Value.Flag.IsLowModule ())
            .ToDictionary (x => x.Value.Flag, x => x.Value);

    public Ship (Information.Item info) : base (info) { }

    protected override void LoadContents (Flags ignoreFlags = Flags.None)
    {
        base.LoadContents (Flags.Pilot);
    }
}