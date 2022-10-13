using System.Collections.Generic;
using System.Linq;
using EVESharp.Database.Inventory;

namespace EVESharp.EVE.Data.Inventory.Items.Types;

public class Ship : ItemInventory
{
    public Dictionary <Flags, ItemEntity> ActiveModules =>
        this.Items
            .Where (x => x.Value.Flag.IsHighModule () || x.Value.Flag.IsMediumModule () || x.Value.Flag.IsLowModule ())
            .ToDictionary (x => x.Value.Flag, x => x.Value);

    public Dictionary <Flags, ItemEntity> RigSlots =>
        this.Items
            .Where (x => x.Value.Flag.IsRigModule ())
            .ToDictionary (x => x.Value.Flag, x => x.Value);

    public Dictionary <Flags, ItemEntity> HighSlotModules =>
        this.Items
            .Where (x => x.Value.Flag.IsHighModule ())
            .ToDictionary (x => x.Value.Flag, x => x.Value);

    public Dictionary <Flags, ItemEntity> MediumSlotModules =>
        this.Items
            .Where (x => x.Value.Flag.IsMediumModule ())
            .ToDictionary (x => x.Value.Flag, x => x.Value);

    public Dictionary <Flags, ItemEntity> LowSlotModules =>
        this.Items
            .Where (x => x.Value.Flag.IsLowModule ())
            .ToDictionary (x => x.Value.Flag, x => x.Value);

    public Ship (Database.Inventory.Types.Information.Item info) : base (info) { }

    protected override void LoadContents (Flags ignoreFlags = Flags.None)
    {
        base.LoadContents (Flags.Pilot);
    }
}