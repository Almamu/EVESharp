using System;
using System.Collections.Generic;
using Node.Inventory.Items;

namespace Node.Inventory
{
    // TODO: REWRITE THIS IN A MORE SENSIBLE WAY, THIS KIND OF NESTED OBJECT IS USUALLY NO GOOD
    public class MetaInventoryManager
    {
        private Dictionary<int, Dictionary<int, ItemInventory>> MetaInventories { get; }

        public MetaInventoryManager()
        {
            this.MetaInventories = new Dictionary<int, Dictionary<int, ItemInventory>>();
        }

        public ItemInventory RegisterMetaInventoryForOwnerID(ItemInventory inventory, int ownerID)
        {
            if (this.MetaInventories.ContainsKey(inventory.ID) == false)
                this.MetaInventories[inventory.ID] = new Dictionary<int, ItemInventory>();

            return this.MetaInventories[inventory.ID][ownerID] = new ItemInventoryByOwnerID(ownerID, inventory);
        }

        public ItemInventory GetOwnerInventoriesAtLocation(int locationID, int ownerID)
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