using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;

namespace EVESharp.Node.Data.Inventory;

public class MetaInventories : Dictionary <int, Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>>>, IMetaInventories
{
    /// <summary>
    /// Event fired when a new meta inventory is created
    /// </summary>
    public IMetaInventories.MetaInventoryItemEvent OnMetaInventoryCreated { get; set; }

    private Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>> MetaInventoriesByOwner { get; } =
        new Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>> ();

    public ItemInventory RegisterMetaInventoryForOwnerID (ItemInventory inventory, int ownerID, Flags flag)
    {
        lock (this)
        lock (this.MetaInventoriesByOwner)
        {
            // ensure the indexes already exists in the dictionary
            this.TryAdd (inventory.ID, new Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>> ());
            this.MetaInventoriesByOwner.TryAdd (ownerID, new Dictionary <Flags, ItemInventoryByOwnerID> ());

            if (this [inventory.ID].TryGetValue (ownerID, out Dictionary <Flags, ItemInventoryByOwnerID> inventories) == false)
                this [inventory.ID] [ownerID] = inventories = new Dictionary <Flags, ItemInventoryByOwnerID> ();

            // only create a new meta inventory if there is none already registered for that owner in that location for that flag
            if (inventories.TryGetValue (flag, out ItemInventoryByOwnerID metaInventory) == false)
            {
                metaInventory = new ItemInventoryByOwnerID (ownerID, flag, inventory);

                this [inventory.ID] [ownerID] [flag] = metaInventory;
                this.MetaInventoriesByOwner [ownerID] [flag]         = metaInventory;

                // fire the creation event
                this.OnMetaInventoryCreated?.Invoke (metaInventory);
            }

            return metaInventory;
        }
    }

    public Dictionary <Flags, ItemInventoryByOwnerID> GetOwnerInventories (int ownerID)
    {
        lock (this.MetaInventoriesByOwner)
        {
            return this.MetaInventoriesByOwner [ownerID];
        }
    }

    public void FreeOwnerInventories (int ownerID)
    {
        lock (this)
        {
            Dictionary <Flags, ItemInventoryByOwnerID> metaInventories = this.GetOwnerInventories (ownerID);

            foreach ((Flags flag, ItemInventoryByOwnerID metaInventory) in metaInventories)
                // unload off the MetaInventories list
                this [metaInventory.ID].Remove (ownerID);
        }

        // free the list of inventories by owner for that owner too
        lock (this.MetaInventoriesByOwner)
        {
            this.MetaInventoriesByOwner.Remove (ownerID);
        }
    }

    public bool GetOwnerInventoryAtLocation (int locationID, int ownerID, Flags flag, out ItemInventoryByOwnerID inventory)
    {
        lock (this)
        {
            inventory = null;

            // get the inventory by it's current flag or by the none flag (used by global inventories)
            return this.TryGetValue (locationID, out Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>> inventoriesByOwner) &&
                   inventoriesByOwner.TryGetValue (ownerID, out Dictionary <Flags, ItemInventoryByOwnerID> ownerInventoriesByFlag) &&
                   (ownerInventoriesByFlag.TryGetValue (flag, out inventory) || ownerInventoriesByFlag.TryGetValue (Flags.None, out inventory));
        }
    }

    public void OnItemLoaded (ItemEntity item)
    {
        if (this.GetOwnerInventoryAtLocation (item.LocationID, item.OwnerID, item.Flag, out ItemInventoryByOwnerID inventory))
            inventory.AddItem (item);
    }

    public void OnItemDestroyed (ItemEntity item)
    {
        if (this.GetOwnerInventoryAtLocation (item.LocationID, item.OwnerID, item.Flag, out ItemInventoryByOwnerID inventory))
            inventory.RemoveItem (item);
    }

    public void OnItemMoved (ItemEntity item, int oldLocationID, int newLocationID, Flags oldFlag, Flags newFlag)
    {
        if (this.GetOwnerInventoryAtLocation (oldLocationID, item.OwnerID, oldFlag, out ItemInventoryByOwnerID origin))
            origin.RemoveItem (item);

        if (this.GetOwnerInventoryAtLocation (newLocationID, item.OwnerID, newFlag, out ItemInventoryByOwnerID destination))
            destination.AddItem (item);
    }
}