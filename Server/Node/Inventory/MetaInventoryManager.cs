using System;
using System.Collections.Generic;
using Node.Inventory.Items;

namespace Node.Inventory
{
    public class MetaInventoryManager
    {
        private Dictionary<int, Dictionary<int, ItemInventoryByOwnerID>> MetaInventories { get; } = new Dictionary<int, Dictionary<int, ItemInventoryByOwnerID>>();

        private Dictionary<int, List<ItemInventoryByOwnerID>> MetaInventoriesByOwner { get; } = new Dictionary<int, List<ItemInventoryByOwnerID>>();

        public ItemInventory RegisterMetaInventoryForOwnerID(ItemInventory inventory, int ownerID)
        {
            lock (this.MetaInventories)
            lock (this.MetaInventoriesByOwner)
            {
                // ensure the indexes already exists in the dictionary
                this.MetaInventories.TryAdd(inventory.ID, new Dictionary<int, ItemInventoryByOwnerID>());
                this.MetaInventoriesByOwner.TryAdd(ownerID, new List<ItemInventoryByOwnerID>());

                // only create a new meta inventory if there is none already registered for that owner in that location
                if (this.MetaInventories[inventory.ID].TryGetValue(ownerID, out ItemInventoryByOwnerID metaInventory) == false)
                {
                    metaInventory = new ItemInventoryByOwnerID(ownerID, inventory);
                    
                    this.MetaInventories[inventory.ID][ownerID] = metaInventory;
                    this.MetaInventoriesByOwner[ownerID].Add(metaInventory);
                }

                return metaInventory;
            }
        }

        public List<ItemInventoryByOwnerID> GetOwnerInventories(int ownerID)
        {
            lock (this.MetaInventoriesByOwner)
                return this.MetaInventoriesByOwner[ownerID];
        }

        public void FreeOwnerInventories(int ownerID)
        {
            lock (this.MetaInventories)
            {
                List<ItemInventoryByOwnerID> metaInventories = this.GetOwnerInventories(ownerID);

                foreach (ItemInventoryByOwnerID metaInventory in metaInventories)
                {
                    // unload off the MetaInventories list
                    this.MetaInventories[metaInventory.ID].Remove(ownerID);
                    // signal the inventory to unload itself
                    metaInventory.Dispose();
                }
            }

            // free the list of inventories by owner for that owner too
            lock (this.MetaInventoriesByOwner)
                this.MetaInventoriesByOwner.Remove(ownerID);
        }

        public bool GetOwnerInventoryAtLocation(int locationID, int ownerID, out ItemInventoryByOwnerID inventory)
        {
            lock (this.MetaInventories)
            {
                inventory = null;
                
                return
                    this.MetaInventories.TryGetValue(locationID, out Dictionary<int, ItemInventoryByOwnerID> inventories) == true &&
                    inventories.TryGetValue(ownerID, out inventory) == true;
            }
        }

        public void OnItemLoaded(ItemEntity item)
        {
            if (this.GetOwnerInventoryAtLocation(item.LocationID, item.OwnerID, out ItemInventoryByOwnerID inventory) == true)
                inventory.AddItem(item);
        }

        public void OnItemDestroyed(ItemEntity item)
        {
            if (this.GetOwnerInventoryAtLocation(item.LocationID, item.OwnerID, out ItemInventoryByOwnerID inventory) == true)
                inventory.RemoveItem(item);
        }

        public void OnItemMoved(ItemEntity item, int oldLocationID, int newLocationID)
        {
            if (this.GetOwnerInventoryAtLocation(oldLocationID, item.OwnerID, out ItemInventoryByOwnerID origin) == true)
                origin.RemoveItem(item);
            
            if (this.GetOwnerInventoryAtLocation(newLocationID, item.OwnerID, out ItemInventoryByOwnerID destination) == true)
                destination.AddItem(item);
        }
    }
}