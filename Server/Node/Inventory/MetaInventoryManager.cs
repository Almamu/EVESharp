using System;
using System.Collections.Generic;
using Node.Inventory.Items;

namespace Node.Inventory
{
    // TODO: REWRITE THIS IN A MORE SENSIBLE WAY, THIS KIND OF NESTED OBJECT IS USUALLY NO GOOD
    public class MetaInventoryManager
    {
        private Dictionary<int, Dictionary<int, ItemInventory>> MetaInventories { get; }
        private Dictionary<int, List<ItemInventory>> MetaInventoriesByOwner { get; }

        public MetaInventoryManager()
        {
            this.MetaInventories = new Dictionary<int, Dictionary<int, ItemInventory>>();
            this.MetaInventoriesByOwner = new Dictionary<int, List<ItemInventory>>();
        }

        public ItemInventory RegisterMetaInventoryForOwnerID(ItemInventory inventory, int ownerID)
        {
            lock (this.MetaInventories)
            lock (this.MetaInventoriesByOwner)
            {
                if (this.MetaInventories.ContainsKey(inventory.ID) == false)
                    this.MetaInventories[inventory.ID] = new Dictionary<int, ItemInventory>();
                if (this.MetaInventoriesByOwner.ContainsKey(ownerID) == false)
                    this.MetaInventoriesByOwner[ownerID] = new List<ItemInventory>();

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

        public ItemInventory GetOwnerInventoriesAtLocation(int locationID, int ownerID)
        {
            lock (this.MetaInventories)
            {
                if (this.MetaInventories.ContainsKey(locationID) == false)
                    throw new ArgumentOutOfRangeException(
                        $"There's no meta inventories registered for this location {locationID}");

                Dictionary<int, ItemInventory> inventoriesForLocation = this.MetaInventories[locationID];

                if (inventoriesForLocation.ContainsKey(ownerID) == false)
                    throw new ArgumentOutOfRangeException(
                        $"There's no meta inventories registered for this location {locationID} and owner {ownerID}");

                return inventoriesForLocation[ownerID];
            }
        }
    }
}