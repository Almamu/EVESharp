using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;

namespace EVESharp.Node.Data.Inventory;

public class MetaInventories : IMetaInventories
{
    /// <summary>
    /// All the registered inventories
    /// </summary>
    private Dictionary <int, Dictionary <int, ItemInventoryByOwnerID>> mInventories = new Dictionary <int, Dictionary <int, ItemInventoryByOwnerID>> ();

    private IDatabase Database { get; }
    private IItems    Items    { get; }
    
    public event Action <ItemInventoryByOwnerID> OnMetaInventoryCreated;

    public MetaInventories (IDatabase database, IItems items)
    {
        Items    = items;
        Database = database;
    }

    public ItemInventoryByOwnerID Create (ItemInventory inventory, int ownerID)
    {
        lock (this.mInventories)
        {
            if (this.mInventories.TryGetValue (inventory.ID, out Dictionary <int, ItemInventoryByOwnerID> byOwners) == false)
                byOwners = this.mInventories [inventory.ID] = new Dictionary <int, ItemInventoryByOwnerID> ();
            if (byOwners.TryGetValue (ownerID, out ItemInventoryByOwnerID ownerInventory) == true)
                return ownerInventory;
        
            ownerInventory = byOwners [ownerID] = new ItemInventoryByOwnerID (ownerID, inventory);

            // setup the load/unload events to load the right data
            ownerInventory.OnInventoryLoad   += this.OnMetaInventoryLoad;
            ownerInventory.OnInventoryUnload += this.OnMetaInventoryUnload;
            
            OnMetaInventoryCreated?.Invoke (ownerInventory);

            return ownerInventory;
        }
    }

    public bool TryGetInventoryForOwner (int locationID, int ownerID, out ItemInventoryByOwnerID inventory)
    {
        inventory = null;

        lock (this.mInventories)
        {
            return this.mInventories.TryGetValue (locationID, out Dictionary <int, ItemInventoryByOwnerID> inventories) == true &&
                   inventories.TryGetValue (ownerID, out inventory) == true;
        }
    }

    private ConcurrentDictionary <int, ItemEntity> OnMetaInventoryLoad (ItemInventory inventory)
    {
        ConcurrentDictionary <int, ItemEntity> result = new ConcurrentDictionary <int, ItemEntity> ();

        foreach (int itemID in Database.InvItemsGetAtLocationForOwner (inventory.ID, inventory.OwnerID))
        {
            ItemEntity item = result [itemID] = Items.LoadItem (itemID);
            item.Parent     = inventory;
        }

        return result;
    }

    private void OnMetaInventoryUnload (ItemInventory inventory)
    {
        if (inventory.ContentsLoaded == false)
            return;

        foreach ((int _, ItemEntity item) in inventory.Items)
            Items.UnloadItem (item);
    }
}