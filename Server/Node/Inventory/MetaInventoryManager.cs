using System;
using System.Collections.Generic;
using Node.Inventory.Items;

namespace Node.Inventory
{
    public class MetaInventoryManager
    {
        private Dictionary<int, Dictionary<int, ItemInventory>> MetaInventories { get; } = new Dictionary<int, Dictionary<int, ItemInventory>>();

        private Dictionary<int, List<ItemInventory>> MetaInventoriesByOwner { get; } = new Dictionary<int, List<ItemInventory>>();

        public ItemInventory RegisterMetaInventoryForOwnerID(ItemInventory inventory, int ownerID)
        {
            lock (this.MetaInventories)
            lock (this.MetaInventoriesByOwner)
            {
                // ensure the indexes already exists in the dictionary
                this.MetaInventories.TryAdd(inventory.ID, new Dictionary<int, ItemInventory>());
                this.MetaInventoriesByOwner.TryAdd(ownerID, new List<ItemInventory>());

                ItemInventoryByOwnerID metaInventory = new ItemInventoryByOwnerID(ownerID, inventory);
                
                this.MetaInventories[inventory.ID][ownerID] = metaInventory;
                this.MetaInventoriesByOwner[ownerID].Add(metaInventory);
        
                return metaInventory;
            }
        }

        public List<ItemInventory> GetOwnerInventories(int ownerID)
        {
            lock (this.MetaInventoriesByOwner)
                return this.MetaInventoriesByOwner[ownerID];
        }

        public void FreeOwnerInventories(int ownerID)
        {
            lock (this.MetaInventories)
            {
                List<ItemInventory> metaInventories = this.GetOwnerInventories(ownerID);

                foreach (ItemInventory metaInventory in metaInventories)
                {
                    // unload off the MetaInventories list
                    this.MetaInventories[metaInventory.ID].Remove(ownerID);
                    // signal the inventory to unload itself
                    metaInventory.Dispose();
                }
            }
        }

        public ItemInventory GetOwnerInventoryAtLocation(int locationID, int ownerID)
        {
            lock (this.MetaInventories)
            {
                if (this.MetaInventories.TryGetValue(locationID, out Dictionary<int, ItemInventory> inventories) == false)
                    throw new ArgumentOutOfRangeException($"There's no meta inventories registered for this location {locationID}");
                if (inventories.TryGetValue(ownerID, out ItemInventory inventory) == false)
                    throw new ArgumentOutOfRangeException($"There's no meta inventories registered for this location {locationID} and owner {ownerID}");

                return inventory;
            }
        }

        public void OnItemLoaded(ItemEntity item)
        {
            try
            {
                this.GetOwnerInventoryAtLocation(item.LocationID, item.OwnerID).AddItem(item);
            }
            catch
            {
                // ignored
            }
        }

        public void OnItemDestroyed(ItemEntity item)
        {
            try
            {
                this.GetOwnerInventoryAtLocation(item.LocationID, item.OwnerID).RemoveItem(item);
            }
            catch
            {
                // ignored
            }
        }

        public void OnItemMoved(ItemEntity item, int oldLocationID, int newLocationID)
        {
            try
            {
                this.GetOwnerInventoryAtLocation(oldLocationID, item.OwnerID).RemoveItem(item);
            }
            catch
            {
                // ignored
            }
            
            try
            {
                this.GetOwnerInventoryAtLocation(newLocationID, item.OwnerID).AddItem(item);
            }
            catch
            {
                // ignored
            }
        }
    }
}