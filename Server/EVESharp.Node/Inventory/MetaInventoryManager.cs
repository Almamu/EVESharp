using System;
using System.Collections.Generic;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.StaticData.Inventory;

namespace EVESharp.Node.Inventory;

public class MetaInventoryManager
{
    public delegate void MetaInventoryItemEvent(ItemInventoryByOwnerID metaInventory);
    private Dictionary<int, Dictionary<int, Dictionary<Flags, ItemInventoryByOwnerID>>> MetaInventories { get; } = new Dictionary<int, Dictionary<int, Dictionary<Flags, ItemInventoryByOwnerID>>>();

    private Dictionary<int, Dictionary<Flags, ItemInventoryByOwnerID>> MetaInventoriesByOwner { get; } = new Dictionary<int, Dictionary<Flags, ItemInventoryByOwnerID>>();

    /// <summary>
    /// Event fired when a new meta inventory is created
    /// </summary>
    public MetaInventoryItemEvent OnMetaInventoryCreated;

    public ItemInventory RegisterMetaInventoryForOwnerID(ItemInventory inventory, int ownerID, Flags flag)
    {
        lock (this.MetaInventories)
        lock (this.MetaInventoriesByOwner)
        {
            // ensure the indexes already exists in the dictionary
            this.MetaInventories.TryAdd(inventory.ID, new Dictionary<int, Dictionary<Flags, ItemInventoryByOwnerID>>());
            this.MetaInventoriesByOwner.TryAdd(ownerID, new Dictionary<Flags, ItemInventoryByOwnerID>());

            if (this.MetaInventories[inventory.ID].TryGetValue(ownerID, out Dictionary<Flags, ItemInventoryByOwnerID> inventories) == false)
                this.MetaInventories[inventory.ID][ownerID] = inventories = new Dictionary<Flags, ItemInventoryByOwnerID>();
                
            // only create a new meta inventory if there is none already registered for that owner in that location for that flag
            if (inventories.TryGetValue(flag, out ItemInventoryByOwnerID metaInventory) == false)
            {
                metaInventory = new ItemInventoryByOwnerID(ownerID, flag, inventory);

                this.MetaInventories[inventory.ID][ownerID][flag] = metaInventory;
                this.MetaInventoriesByOwner[ownerID][flag]        = metaInventory;

                // fire the creation event
                this.OnMetaInventoryCreated?.Invoke(metaInventory);
            }

            return metaInventory;
        }
    }

    public Dictionary<Flags, ItemInventoryByOwnerID> GetOwnerInventories(int ownerID)
    {
        lock (this.MetaInventoriesByOwner)
            return this.MetaInventoriesByOwner[ownerID];
    }

    public void FreeOwnerInventories(int ownerID)
    {
        lock (this.MetaInventories)
        {
            Dictionary<Flags, ItemInventoryByOwnerID> metaInventories = this.GetOwnerInventories(ownerID);

            foreach ((Flags flag, ItemInventoryByOwnerID metaInventory) in metaInventories)
            {
                // unload off the MetaInventories list
                this.MetaInventories[metaInventory.ID].Remove(ownerID);
            }
        }

        // free the list of inventories by owner for that owner too
        lock (this.MetaInventoriesByOwner)
            this.MetaInventoriesByOwner.Remove(ownerID);
    }

    public bool GetOwnerInventoryAtLocation(int locationID, int ownerID, Flags flag, out ItemInventoryByOwnerID inventory)
    {
        lock (this.MetaInventories)
        {
            inventory = null;
                
            // get the inventory by it's current flag or by the none flag (used by global inventories)
            return this.MetaInventories.TryGetValue(locationID, out Dictionary<int, Dictionary<Flags, ItemInventoryByOwnerID>> inventoriesByOwner) == true &&
                   inventoriesByOwner.TryGetValue (ownerID, out Dictionary<Flags, ItemInventoryByOwnerID> ownerInventoriesByFlag) == true &&
                   (ownerInventoriesByFlag.TryGetValue(flag, out inventory) == true || ownerInventoriesByFlag.TryGetValue(Flags.None, out inventory) == true);
        }
    }

    public void OnItemLoaded(ItemEntity item)
    {
        if (this.GetOwnerInventoryAtLocation(item.LocationID, item.OwnerID, item.Flag, out ItemInventoryByOwnerID inventory) == true)
            inventory.AddItem(item);
    }

    public void OnItemDestroyed(ItemEntity item)
    {
        if (this.GetOwnerInventoryAtLocation(item.LocationID, item.OwnerID, item.Flag, out ItemInventoryByOwnerID inventory) == true)
            inventory.RemoveItem(item);
    }

    public void OnItemMoved(ItemEntity item, int oldLocationID, int newLocationID, Flags oldFlag, Flags newFlag)
    {
        if (this.GetOwnerInventoryAtLocation(oldLocationID, item.OwnerID, oldFlag, out ItemInventoryByOwnerID origin) == true)
            origin.RemoveItem(item);
            
        if (this.GetOwnerInventoryAtLocation(newLocationID, item.OwnerID, newFlag, out ItemInventoryByOwnerID destination) == true)
            destination.AddItem(item);
    }
}