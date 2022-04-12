using System;
using EVESharp.EVE.StaticData.Inventory;

namespace EVESharp.Node.Inventory.Items;

public class ItemInventoryByOwnerID : ItemInventory
{
    private int mOwnerID;

    public override int OwnerID
    {
        get => this.mOwnerID;
        set => this.mOwnerID = value;
    }

    public Flags InventoryFlag { get; }

    public ItemInventoryByOwnerID (int ownerID, Flags flag, ItemInventory from) : base (from)
    {
        this.mOwnerID = ownerID;
        InventoryFlag = flag;
    }

    public override void Persist ()
    {
        // persist should do nothing as these are just virtual items
    }

    public override void Dispose ()
    {
        // dispose should do nothing as these are just virtual items
    }

    public override void Destroy ()
    {
        throw new NotSupportedException ("Meta Inventories cannot be destroyed!");
    }
}