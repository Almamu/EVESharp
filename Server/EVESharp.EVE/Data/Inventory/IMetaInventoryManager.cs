using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;

namespace EVESharp.Node.Data.Inventory;

public interface IMetaInventoryManager
{
    public delegate void MetaInventoryItemEvent (ItemInventoryByOwnerID metaInventory);
    /// <summary>
    /// Event fired when a new meta inventory is created
    /// </summary>
    public MetaInventoryItemEvent OnMetaInventoryCreated { get; set; }
    ItemInventory                              RegisterMetaInventoryForOwnerID (ItemInventory inventory, int ownerID, Flags flag);
    Dictionary <Flags, ItemInventoryByOwnerID> GetOwnerInventories (int                       ownerID);
    void                                       FreeOwnerInventories (int                      ownerID);
    bool                                       GetOwnerInventoryAtLocation (int               locationID, int ownerID, Flags flag, out ItemInventoryByOwnerID inventory);
    void                                       OnItemLoaded (ItemEntity                       item);
    void                                       OnItemDestroyed (ItemEntity                    item);
    void OnItemMoved (ItemEntity item, int oldLocationID, int newLocationID, Flags oldFlag, Flags newFlag);
}