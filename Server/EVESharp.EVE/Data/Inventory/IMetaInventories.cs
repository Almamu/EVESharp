using System;
using System.Collections.Generic;
using EVESharp.Database.Inventory;
using EVESharp.EVE.Data.Inventory.Items;

namespace EVESharp.EVE.Data.Inventory;

public interface IMetaInventories
{
    /// <summary>
    /// Event fired when a new meta inventory is created
    /// </summary>
    public event Action <ItemInventoryByOwnerID> OnMetaInventoryCreated;

    /// <summary>
    /// Creates a new meta inventory for the given real inventory and owner
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="ownerID"></param>
    /// <returns></returns>
    ItemInventoryByOwnerID Create (ItemInventory inventory, int ownerID);

    /// <summary>
    /// Searches the meta inventories and provides access to the requested inventory if possible
    /// </summary>
    /// <param name="locationID"></param>
    /// <param name="ownerID"></param>
    /// <param name="inventory"></param>
    /// <returns></returns>
    public bool TryGetInventoryForOwner (int locationID, int ownerID, out ItemInventoryByOwnerID inventory);
}