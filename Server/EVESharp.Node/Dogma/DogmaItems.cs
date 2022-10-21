using System.IO;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Groups;
using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Dogma;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.inventory;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Sessions;

namespace EVESharp.Node.Dogma;

public class DogmaItems : IDogmaItems
{
    private IDogmaNotifications DogmaNotifications { get; }
    
    private IItems Items { get; }
    
    private IMetaInventories MetaInventories { get; }
    
    private EffectsManager EffectsManager { get; }

    public DogmaItems (IDogmaNotifications dogmaNotifications, IItems items, EffectsManager effectsManager, IMetaInventories metaInventories)
    {
        MetaInventories    = metaInventories;
        DogmaNotifications = dogmaNotifications;
        Items              = items;
        EffectsManager     = effectsManager;
    }
    
    public T CreateItem <T> (Type type, ItemEntity owner, ItemInventory location, Flags flag, int quantity = 1, bool singleton = false, bool contraband = false) where T : ItemEntity
    {
        return CreateItem <T> (
            type, owner.ID, location, flag, quantity, singleton, contraband
        );
    }
    
    public T CreateItem <T> (Type type, int ownerID, ItemInventory location, Flags flag, int quantity = 1, bool singleton = false, bool contraband = false) where T : ItemEntity
    {
        ItemEntity newItem = this.Items.CreateSimpleItem (
            type, ownerID, location.ID, flag, quantity, contraband, singleton
        );

        location.AddItem (newItem);
        
        // TODO: DECIDE WHETHER THIS NOTIFICATION MAKES SENSE OR NOT
        DogmaNotifications.QueueMultiEvent (
            ownerID, OnItemChange.BuildNewItemChange (newItem)
        );

        return newItem as T;
    }


    public T CreateItem <T> (Type type, int ownerID, int locationID, Flags flag, int quantity = 1, bool singleton = false, bool contraband = false) where T : ItemEntity
    {
        if (this.TryFindInventory (locationID, ownerID, out ItemInventory location) == false)
            return this.Items.CreateSimpleItem (
                type, ownerID, locationID, flag, quantity, contraband, singleton
            ) as T;

        return CreateItem <T> (type, ownerID, location, flag, quantity, singleton, contraband);
    }

    public T CreateItem <T> (string itemName, Type type, ItemEntity owner, ItemInventory location, Flags flag, int quantity = 1, bool singleton = false, bool   contraband = false) where T : ItemEntity
    {
        return CreateItem <T> (itemName, type, owner.ID, location, flag, quantity, singleton, contraband);
    }
    
    public T CreateItem <T> (string itemName, Type type, int ownerID, ItemInventory location, Flags flag, int quantity = 1, bool singleton = false, bool   contraband = false) where T : ItemEntity
    {
        ItemEntity newItem = this.Items.CreateSimpleItem (
            itemName, type.ID, ownerID, location.ID, flag, quantity, contraband, singleton
        );

        location.AddItem (newItem);
        
        DogmaNotifications.QueueMultiEvent (
            ownerID, OnItemChange.BuildNewItemChange (newItem)
        );

        return newItem as T;
    }

    public T CreateItem <T> (string itemName, Type type, int ownerID, int locationID, Flags flag, int quantity = 1, bool singleton = false, bool   contraband = false) where T : ItemEntity
    {
        if (this.TryFindInventory (locationID, ownerID, out ItemInventory location) == false)
            return this.Items.CreateSimpleItem (
                type, ownerID, locationID, flag, quantity, contraband, singleton
            ) as T;

        return CreateItem <T> (type, ownerID, location, flag, quantity, singleton, contraband);
    }

    public ItemInventory LoadInventory (int inventoryID, int ownerID)
    {
        // try to get the inventory from the metainventories list
        if (this.MetaInventories.TryGetInventoryForOwner (inventoryID, ownerID, out ItemInventoryByOwnerID ownerInventory) == true)
            return ownerInventory;
        
        // inventory not found, check if normal item is loaded and create an inventory off it
        ItemEntity entity = this.Items.LoadItem (inventoryID);

        if (entity is not ItemInventory itemInventory)
            throw new ItemNotContainer (inventoryID);
        if (itemInventory.Type.Group.ID != (int) GroupID.Station && itemInventory.Singleton == false)
            throw new AssembleCCFirst ();
        
        // create a new meta inventory with the required data
        return this.MetaInventories.Create (itemInventory, ownerID);
    }

    public bool TryFindInventory (int inventoryID, int ownerID, out ItemInventory inventory)
    {
        inventory = null;

        if (this.MetaInventories.TryGetInventoryForOwner (inventoryID, ownerID, out ItemInventoryByOwnerID ownerInventory) == false)
            return Items.TryGetItem (inventoryID, out inventory);

        inventory = ownerInventory;
        
        return true;
    }

    public void MoveItem (ItemEntity item, Flags newFlag)
    {
        Flags oldFlag = item.Flag;

        item.Flag = newFlag;
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildLocationChange (item, oldFlag)
        );
    }

    public void MoveItem (ItemEntity item, int newLocationID)
    {
        item.Parent?.RemoveItem (item);
        
        int oldLocationID = item.LocationID;

        item.LocationID = newLocationID;
        item.Persist ();
        
        // get the new parent and add the item to it
        if (TryFindInventory (newLocationID, item.OwnerID, out ItemInventory inventory) == true)
            inventory.AddItem (item);
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildLocationChange (item, oldLocationID)
        );
    }

    public void MoveItem (ItemEntity item, int newLocationID, Flags newFlag)
    {
        int   oldLocationID = item.LocationID;
        Flags oldFlag       = item.Flag;

        item.Parent?.RemoveItem (item);
        
        item.LocationID = newLocationID;
        item.Flag       = newFlag;
        item.Persist ();
        
        // get the new parent and add the item to it
        if (TryFindInventory (newLocationID, item.OwnerID, out ItemInventory inventory) == true)
            inventory.AddItem (item);
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildLocationChange (item, oldFlag, oldLocationID)
        );
    }

    public void MoveItem (ItemEntity item, int newLocationID, int newOwnerID)
    {
        int oldLocationID = item.LocationID;
        int oldOwnerID    = item.OwnerID;
        
        item.Parent?.RemoveItem (item);
        
        // remove the item from the current owner
        if (item.OwnerID != newOwnerID)
        {
            // temporally set the locationID to recycler so it's destroyed for the old player
            item.LocationID = Items.LocationRecycler.ID;
            
            DogmaNotifications.QueueMultiEvent (
                item.OwnerID, OnItemChange.BuildLocationChange (item, oldLocationID)
            );
        }
        
        // update the location
        item.LocationID = newLocationID;
        item.OwnerID    = newOwnerID;
        item.Persist ();
        
        // get the new parent and add the item to it
        if (TryFindInventory (newLocationID, item.OwnerID, out ItemInventory inventory) == true)
            inventory.AddItem (item);
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, oldOwnerID != newOwnerID ? OnItemChange.BuildNewItemChange (item) : OnItemChange.BuildLocationChange (item, oldLocationID)
        );
    }

    public void MoveItem (ItemEntity item, int newLocationID, int newOwnerID, Flags newFlag)
    {
        int   oldOwnerID    = item.OwnerID;
        int   oldLocationID = item.LocationID;
        Flags oldFlag       = item.Flag;
        
        item.Parent?.RemoveItem (item);
        
        // remove the item from the current owner
        if (item.OwnerID != newOwnerID)
        {
            item.LocationID = Items.LocationRecycler.ID;
            
            DogmaNotifications.QueueMultiEvent (
                item.OwnerID, OnItemChange.BuildLocationChange (item, oldLocationID)
            );
        }
        
        // update the location
        item.LocationID = newLocationID;
        item.OwnerID    = newOwnerID;
        item.Flag       = newFlag;
        item.Persist ();
        
        // get the new parent and add the item to it
        if (TryFindInventory (newLocationID, item.OwnerID, out ItemInventory inventory) == true)
            inventory.AddItem (item);

        this.DogmaNotifications.QueueMultiEvent (
            item.OwnerID, oldOwnerID == newOwnerID ? OnItemChange.BuildLocationChange (item, oldFlag, oldLocationID) : OnItemChange.BuildNewItemChange (item)
        );
    }
    
    public ItemEntity SplitStack (ItemEntity item, int splitQuantity)
    {
        if (item.Quantity == splitQuantity)
            return item;

        int oldQuantity = item.Quantity;
        item.Quantity -= splitQuantity;
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildQuantityChange (item, oldQuantity)
        );

        item.Persist ();
        
        return this.CreateItem <ItemEntity> (item.Type, item.OwnerID, item.LocationID, item.Flag, splitQuantity, item.Singleton, item.Contraband);
    }
    
    public ItemEntity SplitStack (ItemEntity item, int splitQuantity, int locationID)
    {
        if (item.Quantity == splitQuantity)
        {
            // the item is really being moved instead of splitted
            MoveItem (item, locationID);

            return item;
        }

        // decrease quantity
        int oldQuantity = item.Quantity;
        item.Quantity -= splitQuantity;
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildQuantityChange (item, oldQuantity)
        );

        item.Persist ();
        
        // create the new item
        return this.CreateItem <ItemEntity> (item.Type, item.OwnerID, locationID, item.Flag, splitQuantity, item.Singleton, item.Contraband);
    }
    
    public ItemEntity SplitStack (ItemEntity item, int splitQuantity, int locationID, int ownerID)
    {
        if (item.Quantity == splitQuantity)
        {
            // the item is really being moved instead of splitted
            MoveItem (item, locationID, ownerID);

            return item;
        }

        // decrease quantity
        int oldQuantity = item.Quantity;
        item.Quantity -= splitQuantity;

        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildQuantityChange (item, oldQuantity)
        );

        item.Persist ();
        
        // create the new item
        return this.CreateItem <ItemEntity> (item.Type, ownerID, locationID, item.Flag, splitQuantity, item.Singleton, item.Contraband);
    }
    
    public ItemEntity SplitStack (ItemEntity item, int splitQuantity, Flags flag)
    {
        if (item.Quantity == splitQuantity)
        {
            // the item is really being moved instead of splitted
            MoveItem (item, flag);

            return item;
        }

        // decrease quantity
        int oldQuantity = item.Quantity;
        item.Quantity -= splitQuantity;
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildQuantityChange (item, oldQuantity)
        );

        item.Persist ();
        
        // create the new item
        return this.CreateItem <ItemEntity> (item.Type, item.OwnerID, item.LocationID, flag, splitQuantity, item.Singleton, item.Contraband);
    }
    
    public ItemEntity SplitStack (ItemEntity item, int splitQuantity, int locationID, Flags flag)
    {
        if (item.Quantity == splitQuantity)
        {
            // the item is really being moved instead of splitted
            MoveItem (item, locationID, flag);

            return item;
        }

        // decrease quantity
        int oldQuantity = item.Quantity;
        item.Quantity -= splitQuantity;
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildQuantityChange (item, oldQuantity)
        );

        item.Persist ();
        
        // create the new item
        return this.CreateItem <ItemEntity> (item.Type, item.OwnerID, locationID, flag, splitQuantity, item.Singleton, item.Contraband);
    }
    
    public ItemEntity SplitStack (ItemEntity item, int splitQuantity, int locationID, int ownerID, Flags flag)
    {
        if (item.Quantity == splitQuantity)
        {
            // the item is really being moved instead of splitted
            MoveItem (item, locationID, ownerID, flag);

            return item;
        }

        // decrease quantity
        int oldQuantity = item.Quantity;
        item.Quantity -= splitQuantity;
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildQuantityChange (item, oldQuantity)
        );

        item.Persist ();
        
        // create the new item
        return this.CreateItem <ItemEntity> (item.Type, ownerID, locationID, flag, splitQuantity, item.Singleton, item.Contraband);
    }

    public void FitInto (ItemEntity item, int locationID, Flags slot, Session session)
    {
        ItemEntity original = null;
        bool wasSingleton = true;
        int originalLocationID = item.LocationID;
        Flags originalFlag = item.Flag;
            
        // cannot be fitted if it's not a module
        if (item is not ShipModule shipModule)
            return;
        
        if (item.Quantity != 1)
        {
            // keep a reference to the old item to undo the changes if required
            original = item;
            // item has to be split first and then moved
            item = SplitStack (item, 1, locationID);
        }
        else
        {
            // move the item to the requested slot
            MoveItem (item, locationID, slot);
        }
        
        // set the singleton if not done already
        if (item.Singleton == false)
        {
            wasSingleton = false;
            item.Singleton = true;
                
            DogmaNotifications.QueueMultiEvent (
                item.OwnerID, OnItemChange.BuildSingletonChange (item, false)
            );
        }

        ItemEffects effects = EffectsManager.GetForItem (shipModule, session);
        
        // throw the effects in and hope they stick
        try
        {
            // apply all passive effects (this also blocks the item fitting if the initialization fails)
            effects?.ApplyPassiveEffects (session);
            // TODO: extra check, ensure that the character has the required skills?

            if (shipModule?.IsRigSlot () == false)
                effects?.ApplyEffect ("online", session);
        }
        catch (UserError)
        {
            effects?.StopApplyingPassiveEffects (session);
            
            // restore the old item again
            if (original is not null)
            {
                original.Quantity++;
                
                DogmaNotifications.QueueMultiEvent (
                    original.OwnerID, OnItemChange.BuildQuantityChange (original, original.Quantity - 1)
                );
                
                // destroy the new item too
                DestroyItem (item);
            }
            else
            {
                // move it back and undo the singleton change
                MoveItem (item, originalLocationID, originalFlag);

                if (wasSingleton == false)
                {
                    item.Singleton = false;
                
                    DogmaNotifications.QueueMultiEvent (
                        item.OwnerID, OnItemChange.BuildSingletonChange (item, true)
                    );
                }
            }
            
            throw;
        }
    }
    
    public void FitInto (ItemEntity item, int locationID, int ownerID, Flags slot, Session session)
    {
        ItemEntity original = null;
        bool wasSingleton = true;
        int originalLocationID = item.LocationID;
        int originalOwnerID = item.OwnerID;
        Flags originalFlag = item.Flag;
            
        // cannot be fitted if it's not a module
        if (item is not ShipModule shipModule)
            return;
        
        if (item.Quantity != 1)
        {
            // keep a reference to the old item to undo the changes if required
            original = item;
            // item has to be split first and then moved
            item = SplitStack (item, 1, item.LocationID);
        }
        
        // set the singleton if not done already
        if (item.Singleton == false)
        {
            wasSingleton = false;
            item.Singleton = true;
                
            DogmaNotifications.QueueMultiEvent (
                item.OwnerID, OnItemChange.BuildSingletonChange (item, false)
            );
        }
        
        // move the item to the requested slot
        MoveItem (item, locationID, ownerID, slot);

        ItemEffects effects = EffectsManager.GetForItem (shipModule, session);
        
        // throw the effects in and hope they stick
        try
        {
            // apply all passive effects (this also blocks the item fitting if the initialization fails)
            effects?.ApplyPassiveEffects (session);
            // TODO: extra check, ensure that the character has the required skills?
        }
        catch (UserError)
        {
            effects?.StopApplyingPassiveEffects (session);
            
            // restore the old item again
            if (original is not null)
            {
                original.Quantity++;
                
                DogmaNotifications.QueueMultiEvent (
                    original.OwnerID, OnItemChange.BuildQuantityChange (original, original.Quantity - 1)
                );
                
                // destroy the new item too
                DestroyItem (item);
            }
            else
            {
                // move it back and undo the singleton change
                MoveItem (item, originalLocationID, originalOwnerID, originalFlag);

                if (wasSingleton == false)
                {
                    item.Singleton = false;
                
                    DogmaNotifications.QueueMultiEvent (
                        item.OwnerID, OnItemChange.BuildSingletonChange (item, true)
                    );
                }
            }
            
            throw;
        }

        if (shipModule?.IsRigSlot () == false)
            effects?.ApplyEffect ("online", session);
    }
    
    public void SetSingleton (ItemEntity item, bool newSingleton)
    {
        if (item.Singleton == newSingleton)
            return;

        bool oldSingleton = item.Singleton;

        item.Singleton = newSingleton;
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildSingletonChange (item, oldSingleton)
        );
    }
    
    public bool Merge (ItemEntity into, ItemEntity from)
    {
        if (into.Singleton || from.Singleton)
            return false;

        if (into.Type.ID != from.Type.ID)
            return false;

        int fromQuantity = from.Quantity;
        into.Quantity += fromQuantity;

        DestroyItem (from);
        
        DogmaNotifications.QueueMultiEvent (
            into.OwnerID, OnItemChange.BuildQuantityChange (into, into.Quantity - fromQuantity)
        );

        into.Persist ();

        return true;
    }

    public bool Merge (ItemEntity into, ItemEntity from, int quantity)
    {
        if (into.Singleton || from.Singleton)
            return false;

        if (into.Type.ID != from.Type.ID)
            return false;

        if (from.Quantity == quantity)
        {
            DestroyItem (from);
        }
        else
        {
            from.Quantity -= quantity;
            
            DogmaNotifications.QueueMultiEvent (
                from.OwnerID, OnItemChange.BuildQuantityChange (from, from.Quantity + quantity)
            );
            
            from.Persist ();
        }
        
        into.Quantity += quantity;

        DogmaNotifications.QueueMultiEvent (
            into.OwnerID, OnItemChange.BuildQuantityChange (into, into.Quantity - quantity)
        );

        into.Persist ();

        return true;
    }

    public void DestroyItem (ItemEntity item)
    {
        item.Parent?.RemoveItem (item);

        int oldLocationID = item.LocationID;
        item.LocationID = Items.LocationRecycler.ID;
        
        DogmaNotifications.QueueMultiEvent (
            item.OwnerID, OnItemChange.BuildLocationChange (item, oldLocationID)
        );

        item.Destroy ();
    }
}