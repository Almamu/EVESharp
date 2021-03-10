using System;
using System.Collections.Generic;
using Node.Database;
using Node.Exceptions;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Notifications;
using Node.Network;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Inventory
{
    public class BoundInventory : BoundService
    {
        private ItemInventory mInventory;
        private ItemFlags mFlag;
        private ItemDB ItemDB { get; }
        private NodeContainer NodeContainer { get; }
        private ItemManager ItemManager { get; }

        public BoundInventory(ItemDB itemDB, ItemInventory item, ItemManager itemManager, NodeContainer nodeContainer, BoundServiceManager manager, Client client) : base(manager, client)
        {
            this.mInventory = item;
            this.mFlag = ItemFlags.None;
            this.ItemDB = itemDB;
            this.ItemManager = itemManager;
            this.NodeContainer = nodeContainer;
        }

        public BoundInventory(ItemDB itemDB, ItemInventory item, ItemFlags flag, ItemManager itemManager, NodeContainer nodeContainer, BoundServiceManager manager, Client client) : base(manager, client)
        {
            this.mInventory = item;
            this.mFlag = flag;
            this.ItemDB = itemDB;
            this.ItemManager = itemManager;
            this.NodeContainer = nodeContainer;
        }

        public PyDataType List(CallInformation call)
        {        
            CRowset result = new CRowset(ItemEntity.sEntityItemDescriptor);

            foreach (KeyValuePair<int, ItemEntity> pair in this.mInventory.Items)
                if (this.mFlag == ItemFlags.None || pair.Value.Flag == this.mFlag)
                    result.Add(pair.Value.GetEntityRow());

            return result;
        }

        public PyDataType ListStations(PyInteger blueprintsOnly, PyInteger forCorp, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            // TODO: take into account blueprintsOnly
            if (forCorp == 1)
                return this.ItemDB.ListStations(call.Client.CorporationID);
            else
                return this.ItemDB.ListStations(callerCharacterID);
        }

        public PyDataType ListStationItems(PyInteger stationID, CallInformation call)
        {
            return this.ItemDB.ListStationItems(stationID, call.Client.EnsureCharacterIsSelected());
        }
        
        public PyDataType GetItem(CallInformation call)
        {
            return this.mInventory.GetEntityRow();
        }

        public PyDataType Add(PyInteger itemID, CallInformation call)
        {
            if (itemID == call.Client.ShipID)
                throw new CantMoveActiveShip();
            
            // the item has to be moved to this inventory completely
            if (this.ItemManager.IsItemLoaded(itemID) == false)
            {
                ItemEntity item = this.ItemManager.LoadItem(itemID);

                // get old information
                int oldLocationID = item.LocationID;
                ItemFlags oldFlag = item.Flag;
                
                // set the new location for the item
                item.LocationID = this.mInventory.ID;
                item.Flag = this.mFlag;

                call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(item, oldFlag, oldLocationID));
                
                // finally add the item to this inventory
                this.mInventory.AddItem(item);

                item.Persist();
            }
            else
            {
                ItemEntity item = this.ItemManager.GetItem(itemID);
                
                // remove item off the old inventory
                if (this.ItemManager.IsItemLoaded(item.LocationID) == true)
                {
                    ItemInventory inventory = this.ItemManager.GetItem(item.LocationID) as ItemInventory;

                    inventory.RemoveItem(item);
                }
                
                // remove item off the meta inventories
                try
                {
                    this.ItemManager.MetaInventoryManager
                        .GetOwnerInventoriesAtLocation(item.LocationID, item.OwnerID)
                        .RemoveItem(item);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                
                // get old information
                int oldLocationID = item.LocationID;
                ItemFlags oldFlag = item.Flag;
                
                // set the new location for the item
                item.LocationID = this.mInventory.ID;
                item.Flag = this.mFlag;

                call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(item, oldFlag, oldLocationID));
                
                // finally add the item to this inventory
                this.mInventory.AddItem(item);

                item.Persist();
            }

            return null;
        }

        public PyDataType Add(PyInteger itemID, PyInteger quantity, PyInteger flag, CallInformation call)
        {
            if (itemID == call.Client.ShipID)
                throw new CantMoveActiveShip();

            // TODO: ADD CONSTRAINTS CHECKS FOR THE FLAG
            if (this.ItemManager.IsItemLoaded(itemID) == false)
            {
                // not loaded item, the steps are simpler as the server doesn't really know much about it
                ItemEntity item = this.ItemManager.LoadItem(itemID);

                if (quantity < item.Quantity)
                {
                    // subtract the quantities and create the new item
                    item.Quantity -= quantity;
                    item.Persist();
                    
                    // create a new item with the same specs as the original
                    ItemEntity clone = this.ItemManager.CreateSimpleItem(item.Type, item.OwnerID, this.mInventory.ID, (ItemFlags) (int) flag, quantity,
                        item.Contraband, item.Singleton);
                    
                    // persist it to the database
                    clone.Persist();
                    // notify the client of the new item
                    call.Client.NotifyMultiEvent(OnItemChange.BuildNewItemChange(clone));
                    // and notify the amount change
                    call.Client.NotifyMultiEvent(OnItemChange.BuildQuantityChange(item, item.Quantity + quantity));
                }
                else
                {
                    int oldLocation = item.LocationID;
                    ItemFlags oldFlag = item.Flag;
                    
                    // simple, move the item
                    item.LocationID = this.mInventory.ID;
                    item.Flag = (ItemFlags) (int) flag;
                    
                    item.Persist();
                        
                    // add it to the inventory
                    this.mInventory.AddItem(item);
                    
                    // notify the client
                    call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(item, oldFlag, oldLocation));
                }
            }
            else
            {
                ItemEntity item = this.ItemManager.GetItem(itemID);

                // ensure there's enough quantity in the stack to split it
                if (quantity > item.Quantity)
                    return null;

                if (quantity == item.Quantity)
                {
                    // the item is being moved completely, the easiest way is to remove from the old inventory
                    // and put it in the new one
                    
                    // this means we require access to the original inventory to remove the item from there
                    // and thus we have to be extra careful
                    if (this.ItemManager.IsItemLoaded(item.LocationID) == true)
                    {
                        ItemInventory inventory = this.ItemManager.GetItem(item.LocationID) as ItemInventory;

                        // remove the item from the inventory
                        inventory.RemoveItem(item);
                        
                        // TODO: TAKE INTO ACCOUNT META INVENTORIES
                    }

                    int oldLocationID = item.LocationID;
                    ItemFlags oldFlag = item.Flag;

                    item.LocationID = this.mInventory.ID;
                    item.Flag = (ItemFlags) (int) flag;

                    item.Persist();
                    this.mInventory.AddItem(item);
                    // notify the client of the location change
                    call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(item, oldFlag, oldLocationID));
                }
                else
                {
                    // create a new item with the same specs as the original
                    ItemEntity clone = this.ItemManager.CreateSimpleItem(item.Type, item.OwnerID, this.mInventory.ID, (ItemFlags) (int) flag, quantity,
                        item.Contraband, item.Singleton);
            
                    // subtract the quantity off the original item
                    item.Quantity -= quantity;
                    // notify the changes to the client
                    call.Client.NotifyMultiEvent(OnItemChange.BuildQuantityChange(item, item.Quantity + quantity));
                    call.Client.NotifyMultiEvent(OnItemChange.BuildNewItemChange(clone));
                    // persist the item changes in the database
                    clone.Persist();
                    item.Persist();
                }
            }
            
            return null;
        }

        public PyDataType MultiMerge(PyList merges, CallInformation call)
        {
            foreach (PyDataType merge in merges)
            {
                // ignore wrong multimerge formats
                if (merge is PyTuple == false)
                    continue;

                PyTuple tuple = merge as PyTuple;

                if (tuple[0] is PyInteger == false || tuple[1] is PyInteger == false || tuple[2] is PyInteger == false)
                    continue;

                PyInteger fromItemID = tuple[0] as PyInteger;
                PyInteger toItemID = tuple[1] as PyInteger;
                PyInteger quantity = tuple[2] as PyInteger;

                if (this.mInventory.Items.ContainsKey(toItemID) == false)
                    continue;

                ItemEntity fromItem = this.ItemManager.GetItem(fromItemID);
                ItemEntity toItem = this.mInventory.Items[toItemID];

                // ignore singleton items
                if (fromItem.Singleton == true || toItem.Singleton == true)
                    continue;
                
                // if we're fully merging two stacks, just remove one item
                if (quantity == fromItem.Quantity)
                {
                    int oldLocationID = fromItem.LocationID;
                    
                    // remove the item
                    this.ItemManager.DestroyItem(fromItem);
                    // notify the client about the item too
                    call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(fromItem, oldLocationID));
                }
                else
                {
                    // change the item's quantity
                    fromItem.Quantity -= quantity;
                    // notify the client about the change
                    call.Client.NotifyMultiEvent(OnItemChange.BuildQuantityChange(fromItem, fromItem.Quantity + quantity));
                    fromItem.Persist();
                }

                toItem.Quantity += quantity;
                call.Client.NotifyMultiEvent(OnItemChange.BuildQuantityChange(toItem, toItem.Quantity - quantity));
                toItem.Persist();
            }
            
            return null;
        }

        public PyDataType StackAll(PyString password, CallInformation call)
        {
            throw new NotImplementedException("Stacking on passworded containers is not supported yet!");
        }

        public PyDataType StackAll(PyInteger locationFlag, CallInformation call)
        {
            // TODO: ADD CONSTRAINTS CHECKS FOR THE LOCATIONFLAG
            foreach (int firstItemID in this.mInventory.Items.Keys)
            {
                ItemEntity firstItem = this.mInventory.Items[firstItemID];
                
                // singleton items are not even checked
                if (firstItem.Singleton == true || firstItem.Flag != (ItemFlags) (int) locationFlag)
                    continue;
                
                foreach (int secondItemID in this.mInventory.Items.Keys)
                {
                    // ignore the same itemID as they cannot really be merged
                    if (firstItemID == secondItemID)
                        continue;
                    
                    ItemEntity secondItem = this.mInventory.Items[secondItemID];
                    
                    // ignore the item if it's singleton
                    if (secondItem.Singleton == true || secondItem.Flag != (ItemFlags) (int) locationFlag)
                        continue;
                    // ignore the item check if they're not the same type ID
                    if (firstItem.Type.ID != secondItem.Type.ID)
                        continue;
                    int oldQuantity = secondItem.Quantity;
                    // add the quantity of the first item to the second
                    secondItem.Quantity += firstItem.Quantity;
                    // also create the notification for the user
                    call.Client.NotifyMultiEvent(OnItemChange.BuildQuantityChange(secondItem, oldQuantity));
                    this.ItemManager.DestroyItem(firstItem);
                    // notify the client about the item too
                    call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(firstItem, firstItem.Flag, secondItem.LocationID));
                    // ensure the second item is saved to database too
                    secondItem.Persist();
                    // finally break this loop as the merge was already done
                    break;
                }
            }

            return null;
        }
        
        public PyDataType StackAll(CallInformation call)
        {
            foreach (int firstItemID in this.mInventory.Items.Keys)
            {
                ItemEntity firstItem = this.mInventory.Items[firstItemID];
                
                // singleton items are not even checked
                if (firstItem.Singleton == true)
                    continue;
                
                foreach (int secondItemID in this.mInventory.Items.Keys)
                {
                    // ignore the same itemID as they cannot really be merged
                    if (firstItemID == secondItemID)
                        continue;
                    
                    ItemEntity secondItem = this.mInventory.Items[secondItemID];
                    
                    // ignore the item if it's singleton
                    if (secondItem.Singleton == true)
                        continue;
                    // ignore the item check if they're not the same type ID
                    if (firstItem.Type.ID != secondItem.Type.ID)
                        continue;
                    int oldQuantity = secondItem.Quantity;
                    // add the quantity of the first item to the second
                    secondItem.Quantity += firstItem.Quantity;
                    // also create the notification for the user
                    call.Client.NotifyMultiEvent(OnItemChange.BuildQuantityChange(secondItem, oldQuantity));
                    this.ItemManager.DestroyItem(firstItem);
                    // notify the client about the item too
                    call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(firstItem, firstItem.Flag, secondItem.LocationID));
                    // ensure the second item is saved to database too
                    secondItem.Persist();
                    // finally break this loop as the merge was already done
                    break;
                }
            }

            return null;
        }

        public static PyDataType BindInventory(ItemDB itemDB, ItemInventory item, ItemFlags flag, ItemManager itemManager, NodeContainer nodeContainer, BoundServiceManager boundServiceManager, Client client)
        {
            BoundService instance = new BoundInventory(itemDB, item, flag, itemManager, nodeContainer, boundServiceManager, client);
            // bind the service
            int boundID = boundServiceManager.BoundService(instance);
            // build the bound service string
            string boundServiceStr = boundServiceManager.BuildBoundServiceString(boundID);

            // TODO: the expiration time is 1 day, might be better to properly support this?
            // TODO: investigate these a bit more closely in the future
            // TODO: i'm not so sure about the expiration time
            PyTuple boundServiceInformation = new PyTuple(new PyDataType[]
            {
                boundServiceStr, DateTime.UtcNow.Add(TimeSpan.FromDays(1)).ToFileTime()
            });

            // after the service is bound the call can be run (if required)
            return new PySubStruct(new PySubStream(boundServiceInformation));
        }
    }
}