using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Dogma;

public interface IDogmaItems
{
    /// <summary>
    /// Creates a new item
    /// </summary>
    /// <param name="type"></param>
    /// <param name="owner"></param>
    /// <param name="location"></param>
    /// <param name="flag"></param>
    /// <param name="quantity"></param>
    /// <param name="singleton"></param>
    /// <param name="contraband"></param>
    T CreateItem <T> (Type type, ItemEntity owner, ItemInventory location, Flags flag, int quantity = 1, bool singleton = false, bool contraband = false) where T : ItemEntity;
    
    /// <summary>
    /// Creates a new item
    /// </summary>
    /// <param name="type"></param>
    /// <param name="owner"></param>
    /// <param name="location"></param>
    /// <param name="flag"></param>
    /// <param name="quantity"></param>
    /// <param name="singleton"></param>
    /// <param name="contraband"></param>
    T CreateItem <T> (Type type, int ownerID, ItemInventory location, Flags flag, int quantity = 1, bool singleton = false, bool contraband = false) where T : ItemEntity;

    /// <summary>
    /// Creates a new item
    /// </summary>
    /// <param name="type"></param>
    /// <param name="ownerID"></param>
    /// <param name="locationID"></param>
    /// <param name="flag"></param>
    /// <param name="quantity"></param>
    /// <param name="singleton"></param>
    /// <param name="contraband"></param>
    T CreateItem <T> (Type type, int ownerID, int locationID, Flags flag, int quantity = 1, bool singleton = false, bool contraband = false) where T : ItemEntity;
    
    /// <summary>
    /// Creates a new item
    /// </summary>
    /// <param name="itemName"></param>
    /// <param name="type"></param>
    /// <param name="owner"></param>
    /// <param name="location"></param>
    /// <param name="flag"></param>
    /// <param name="quantity"></param>
    /// <param name="singleton"></param>
    /// <param name="contraband"></param>
    T CreateItem <T> (string itemName, Type type, ItemEntity owner, ItemInventory location, Flags flag, int quantity = 1, bool singleton = false, bool contraband = false) where T : ItemEntity;
    
    /// <summary>
    /// Creates a new item
    /// </summary>
    /// <param name="itemName"></param>
    /// <param name="type"></param>
    /// <param name="ownerID"></param>
    /// <param name="locationID"></param>
    /// <param name="flag"></param>
    /// <param name="quantity"></param>
    /// <param name="singleton"></param>
    /// <param name="contraband"></param>
    T CreateItem <T> (string itemName, Type type, int ownerID, ItemInventory location, Flags flag, int quantity = 1, bool singleton = false, bool contraband = false) where T : ItemEntity;
    
    /// <summary>
    /// Creates a new item
    /// </summary>
    /// <param name="itemName"></param>
    /// <param name="type"></param>
    /// <param name="ownerID"></param>
    /// <param name="locationID"></param>
    /// <param name="flag"></param>
    /// <param name="quantity"></param>
    /// <param name="singleton"></param>
    /// <param name="contraband"></param>
    T CreateItem <T> (string itemName, Type type, int ownerID, int locationID, Flags flag, int quantity = 1, bool singleton = false, bool contraband = false) where T : ItemEntity;
    
    /// <summary>
    /// Loads a new inventory into dogma for it's usage
    /// </summary>
    /// <param name="inventoryID"></param>
    /// <param name="ownerID"></param>
    /// <returns></returns>
    ItemInventory LoadInventory (int inventoryID, int ownerID);
    
    /// <summary>
    /// Searches the dogma to try and retrieve an item that handles the given inventoryID and ownerID
    /// </summary>
    /// <param name="inventoryID"></param>
    /// <param name="ownerID"></param>
    /// <param name="inventory"></param>
    /// <returns></returns>
    bool TryFindInventory (int inventoryID, int ownerID, out ItemInventory inventory);

    /// <summary>
    /// Moves the given item to the specified new flag
    /// </summary>
    /// <param name="item"></param>
    /// <param name="newFlag"></param>
    void MoveItem (ItemEntity item, Flags newFlag);
    
    /// <summary>
    /// Moves the given item to the specified new locationID
    /// </summary>
    /// <param name="item"></param>
    /// <param name="newLocationID"></param>
    void MoveItem (ItemEntity item, int newLocationID);
    
    /// <summary>
    /// Moves the given item to the specified new locationID and flag
    /// </summary>
    /// <param name="item"></param>
    /// <param name="newLocationID"></param>
    /// <param name="newFlag"></param>
    void MoveItem (ItemEntity item, int newLocationID, Flags newFlag);
    
    /// <summary>
    /// Moves the given item to the specified new locationID and ownerID
    /// </summary>
    /// <param name="item"></param>
    /// <param name="newLocationID"></param>
    /// <param name="newOwnerID"></param>
    void MoveItem (ItemEntity item, int newLocationID, int newOwnerID);
    
    /// <summary>
    /// Moves the given item to the specified new locationID, ownerID and flag
    /// </summary>
    /// <param name="item"></param>
    /// <param name="newLocationID"></param>
    /// <param name="newOwnerID"></param>
    /// <param name="newFlag"></param>
    void MoveItem (ItemEntity item, int newLocationID, int newOwnerID, Flags newFlag);

    /// <summary>
    /// Splits an item's quantity
    /// </summary>
    /// <param name="item"></param>
    /// <param name="splitQuantity"></param>
    /// <returns></returns>
    ItemEntity SplitStack (ItemEntity item, int splitQuantity);

    /// <summary>
    /// Splits an item's quantity
    /// </summary>
    /// <param name="item"></param>
    /// <param name="splitQuantity"></param>
    /// <param name="locationID"></param>
    /// <returns></returns>
    ItemEntity SplitStack (ItemEntity item, int splitQuantity, int locationID);

    /// <summary>
    /// Splits an item's quantity
    /// </summary>
    /// <param name="item"></param>
    /// <param name="splitQuantity"></param>
    /// <param name="locationID"></param>
    /// <param name="ownerID"></param>
    /// <returns></returns>
    ItemEntity SplitStack (ItemEntity item, int splitQuantity, int locationID, int ownerID);

    /// <summary>
    /// Splits an item's quantity
    /// </summary>
    /// <param name="item"></param>
    /// <param name="splitQuantity"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    ItemEntity SplitStack (ItemEntity item, int splitQuantity, Flags flag);

    /// <summary>
    /// Splits an item's quantity
    /// </summary>
    /// <param name="item"></param>
    /// <param name="splitQuantity"></param>
    /// <param name="locationID"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    ItemEntity SplitStack (ItemEntity item, int splitQuantity, int locationID, Flags flag);

    /// <summary>
    /// Splits an item's quantity
    /// </summary>
    /// <param name="item"></param>
    /// <param name="splitQuantity"></param>
    /// <param name="locationID"></param>
    /// <param name="ownerID"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    ItemEntity SplitStack (ItemEntity item, int splitQuantity, int locationID, int ownerID, Flags flag);

    /// <summary>
    /// Fits a ship module into the given locationID
    /// </summary>
    /// <param name="item"></param>
    /// <param name="locationID"></param>
    /// <param name="slot"></param>
    /// <param name="session"></param>
    void FitInto (ItemEntity item, int locationID, Flags slot, Session session);

    /// <summary>
    /// Fits a ship module into the given locationID
    /// </summary>
    /// <param name="item"></param>
    /// <param name="locationID"></param>
    /// <param name="ownerID"></param>
    /// <param name="slot"></param>
    /// <param name="session"></param>
    void FitInto (ItemEntity item, int locationID, int ownerID, Flags slot, Session session);

    /// <summary>
    /// Merges two items
    /// </summary>
    /// <param name="into"></param>
    /// <param name="from"></param>
    /// <returns>If the merge happened or not</returns>
    bool Merge (ItemEntity into, ItemEntity from);

    /// <summary>
    /// Merges two items
    /// </summary>
    /// <param name="into"></param>
    /// <param name="from"></param>
    /// <returns>If the merge happened or not</returns>
    bool Merge (ItemEntity into, ItemEntity from, int quantity);
    
    /// <summary>
    /// Destroys the given item
    /// </summary>
    /// <param name="item"></param>
    void DestroyItem (ItemEntity item);
}