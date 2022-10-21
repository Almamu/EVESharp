using System;
using EVESharp.Database.Inventory;

namespace EVESharp.EVE.Data.Inventory.Items;

public class ItemInventoryByOwnerID : ItemInventory
{
    private int mOwnerID;

    public override int OwnerID
    {
        get => this.mOwnerID;
        set => this.mOwnerID = value;
    }


    public ItemInventoryByOwnerID (int ownerID, ItemInventory from) : base (from)
    {
        this.mOwnerID      = ownerID;
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