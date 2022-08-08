using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.Node.Inventory.Items;

namespace EVESharp.Node.Inventory;

public class MetaInventoryManager
{
    public delegate void MetaInventoryItemEvent (ItemInventoryByOwnerID metaInventory);

    /// <summary>
    /// Event fired when a new meta inventory is created
    /// </summary>
    public MetaInventoryItemEvent OnMetaInventoryCreated;
    private Dictionary <int, Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>>> MetaInventories { get; } =
        new Dictionary <int, Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>>> ();

    private Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>> MetaInventoriesByOwner { get; } =
        new Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>> ();

    public ItemInventory RegisterMetaInventoryForOwnerID (ItemInventory inventory, int ownerID, Flags flag)
    {
        lock (MetaInventories)
        lock (MetaInventoriesByOwner)
        {
            // ensure the indexes already exists in the dictionary
            MetaInventories.TryAdd (inventory.ID, new Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>> ());
            MetaInventoriesByOwner.TryAdd (ownerID, new Dictionary <Flags, ItemInventoryByOwnerID> ());

            if (MetaInventories [inventory.ID].TryGetValue (ownerID, out Dictionary <Flags, ItemInventoryByOwnerID> inventories) == false)
                MetaInventories [inventory.ID] [ownerID] = inventories = new Dictionary <Flags, ItemInventoryByOwnerID> ();

            // only create a new meta inventory if there is none already registered for that owner in that location for that flag
            if (inventories.TryGetValue (flag, out ItemInventoryByOwnerID metaInventory) == false)
            {
                metaInventory = new ItemInventoryByOwnerID (ownerID, flag, inventory);

                MetaInventories [inventory.ID] [ownerID] [flag] = metaInventory;
                MetaInventoriesByOwner [ownerID] [flag]         = metaInventory;

                // fire the creation event
                this.OnMetaInventoryCreated?.Invoke (metaInventory);
            }

            return metaInventory;
        }
    }

    public Dictionary <Flags, ItemInventoryByOwnerID> GetOwnerInventories (int ownerID)
    {
        lock (MetaInventoriesByOwner)
        {
            return MetaInventoriesByOwner [ownerID];
        }
    }

    public void FreeOwnerInventories (int ownerID)
    {
        lock (MetaInventories)
        {
            Dictionary <Flags, ItemInventoryByOwnerID> metaInventories = this.GetOwnerInventories (ownerID);

            foreach ((Flags flag, ItemInventoryByOwnerID metaInventory) in metaInventories)
                // unload off the MetaInventories list
                MetaInventories [metaInventory.ID].Remove (ownerID);
        }

        // free the list of inventories by owner for that owner too
        lock (MetaInventoriesByOwner)
        {
            MetaInventoriesByOwner.Remove (ownerID);
        }
    }

    public bool GetOwnerInventoryAtLocation (int locationID, int ownerID, Flags flag, out ItemInventoryByOwnerID inventory)
    {
        lock (MetaInventories)
        {
            inventory = null;

            // get the inventory by it's current flag or by the none flag (used by global inventories)
            return MetaInventories.TryGetValue (locationID, out Dictionary <int, Dictionary <Flags, ItemInventoryByOwnerID>> inventoriesByOwner) &&
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