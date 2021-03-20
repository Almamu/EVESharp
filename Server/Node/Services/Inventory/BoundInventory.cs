using System;
using System.Collections.Generic;
using Node.Database;
using Node.Exceptions;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Notifications;
using Node.Network;
using PythonTypes.Types.Collections;
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

            foreach ((int _, ItemEntity item) in this.mInventory.Items)
                if (this.mFlag == ItemFlags.None || item.Flag == this.mFlag)
                    result.Add(item.GetEntityRow());

            return result;
        }

        public PyDataType ListStations(PyInteger blueprintsOnly, PyInteger forCorp, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            // TODO: take into account blueprintsOnly
            if (forCorp == 1)
                return this.ItemDB.ListStations(call.Client.CorporationID);
            
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

        private void MoveItemHere(ItemEntity item, ItemFlags newFlag, Client relatedClient)
        {
            // remove item off the old inventory if required
            if (this.ItemManager.TryGetItem(item.LocationID, out ItemInventory inventory) == true)
                inventory.RemoveItem(item);
            
            // get the old location stored as it'll be used in the notifications
            int oldLocation = item.LocationID;
            ItemFlags oldFlag = item.Flag;
            
            // set the new location for the item
            item.LocationID = this.mInventory.ID;
            item.Flag = newFlag;
            
            // notify the character about the change
            Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(item, oldFlag, oldLocation));
            // update meta inventories too
            this.ItemManager.MetaInventoryManager.OnItemMoved(item, oldLocation, this.mInventory.ID);
            
            // finally persist the item changes
            item.Persist();
        }

        public PyDataType Add(PyInteger itemID, CallInformation call)
        {
            if (itemID == call.Client.ShipID)
                throw new CantMoveActiveShip();

            this.MoveItemHere(this.ItemManager.GetItem(itemID), this.mFlag, call.Client);

            return null;
        }

        public PyDataType Add(PyInteger itemID, PyInteger quantity, CallInformation call)
        {
            if (itemID == call.Client.ShipID)
                throw new CantMoveActiveShip();

            this.MoveItemHere(this.ItemManager.GetItem(itemID), this.mFlag, call.Client);

            return null;
        }

        public PyDataType Add(PyInteger itemID, PyInteger quantity, PyInteger flag, CallInformation call)
        {
            if (itemID == call.Client.ShipID)
                throw new CantMoveActiveShip();

            // TODO: ADD CONSTRAINTS CHECKS FOR THE FLAG
            ItemEntity item = this.ItemManager.GetItem(itemID);

            // ensure there's enough quantity in the stack to split it
            if (quantity > item.Quantity)
                return null;

            if (quantity == item.Quantity)
            {
                // the item is being moved completely, the easiest way is to remove from the old inventory
                // and put it in the new one
                this.MoveItemHere(item, (ItemFlags) (int) flag, call.Client);
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

                if (this.mInventory.Items.TryGetValue(toItemID, out ItemEntity toItem) == false)
                    continue;

                ItemEntity fromItem = this.ItemManager.GetItem(fromItemID);

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
            foreach ((int firstItemID, ItemEntity firstItem) in this.mInventory.Items)
            {
                // singleton items are not even checked
                if (firstItem.Singleton == true || firstItem.Flag != (ItemFlags) (int) locationFlag)
                    continue;
                
                foreach ((int secondItemID, ItemEntity secondItem) in this.mInventory.Items)
                {
                    // ignore the same itemID as they cannot really be merged
                    if (firstItemID == secondItemID)
                        continue;
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
            foreach ((int firstItemID, ItemEntity firstItem) in this.mInventory.Items)
            {
                // singleton items are not even checked
                if (firstItem.Singleton == true)
                    continue;
                
                // there's some specific groups that cannot be assembled even if they're singleton
                
                foreach ((int secondItemID, ItemEntity secondItem) in this.mInventory.Items)
                {
                    // ignore the same itemID as they cannot really be merged
                    if (firstItemID == secondItemID)
                        continue;
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

        public PyDataType BreakPlasticWrap(PyInteger crateID, CallInformation call)
        {
            // TODO: ensure this item is a plastic wrap and mark the contract as void
            return null;
        }
    }
}