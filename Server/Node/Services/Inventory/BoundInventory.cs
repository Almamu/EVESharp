using System;
using System.Collections.Generic;
using System.Linq;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.inventory;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Inventory.Notifications;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
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

        private void PreMoveItemCheck(ItemEntity item, ItemFlags flag, double quantityToMove)
        {
            if (this.mInventory.Type.ID == (int) ItemTypes.Capsule)
            {
                throw new CantTakeInSpaceCapsule();
            }
            
            // perform checks only on cargo
            if (this.mInventory is Ship ship && flag == ItemFlags.Cargo)
            {
                // check destination cargo
                double currentVolume =
                    ship.Items.Sum(x => (x.Value.Flag != flag) ? 0.0 : x.Value.Quantity * x.Value.Attributes[AttributeEnum.volume]);

                double newVolume = item.Attributes[AttributeEnum.volume] * quantityToMove + currentVolume;
                double volumeMultiplier = ship.ActiveModules
                    .Select(x => x.Value.Attributes[AttributeEnum.cargoCapacityMultiplier])
                    .Aggregate(1.0, (a, x) => x * a);
                double maxVolume = this.mInventory.Attributes[AttributeEnum.capacity] * volumeMultiplier;

                if (newVolume > maxVolume)
                    throw new NotEnoughCargoSpace(currentVolume, this.mInventory.Attributes[AttributeEnum.capacity] - currentVolume);
            }
        }

        private void MoveItemHere(ItemEntity item, ItemFlags newFlag)
        {
            // remove item off the old inventory if required
            if (this.ItemManager.TryGetItem(item.LocationID, out ItemInventory inventory) == true)
                inventory.RemoveItem(item);
            
            // get the old location stored as it'll be used in the notifications
            int oldLocation = item.LocationID;
            ItemFlags oldFlag = item.Flag;
            
            // special situation, if the old location is a module slot ensure the item is first offlined
            if (oldFlag == ItemFlags.HiSlot0 || oldFlag == ItemFlags.HiSlot1 || oldFlag == ItemFlags.HiSlot2 ||
                oldFlag == ItemFlags.HiSlot3 || oldFlag == ItemFlags.HiSlot4 || oldFlag == ItemFlags.HiSlot5 ||
                oldFlag == ItemFlags.HiSlot6 || oldFlag == ItemFlags.HiSlot7 || oldFlag == ItemFlags.MedSlot0 ||
                oldFlag == ItemFlags.MedSlot1 || oldFlag == ItemFlags.MedSlot2 || oldFlag == ItemFlags.MedSlot3 ||
                oldFlag == ItemFlags.MedSlot4 || oldFlag == ItemFlags.MedSlot5 || oldFlag == ItemFlags.MedSlot6 ||
                oldFlag == ItemFlags.MedSlot7 || oldFlag == ItemFlags.LoSlot0 || oldFlag == ItemFlags.LoSlot1 ||
                oldFlag == ItemFlags.LoSlot2 || oldFlag == ItemFlags.LoSlot3 || oldFlag == ItemFlags.LoSlot4 ||
                oldFlag == ItemFlags.LoSlot5 || oldFlag == ItemFlags.LoSlot6 || oldFlag == ItemFlags.LoSlot7)
            {
                if (item is ShipModule module)
                    module.PutOffline(Client);
            }
            
            // extra special situation, is the new flag an autofit one?
            if (newFlag == ItemFlags.AutoFit)
            {
                // determine where to put the item
                
            }
            
            // special situation, if the new location is a module slot ensure the item is a singleton (TODO: HANDLE CHARGES TOO)
            if (newFlag == ItemFlags.HiSlot0 || newFlag == ItemFlags.HiSlot1 || newFlag == ItemFlags.HiSlot2 ||
                newFlag == ItemFlags.HiSlot3 || newFlag == ItemFlags.HiSlot4 || newFlag == ItemFlags.HiSlot5 ||
                newFlag == ItemFlags.HiSlot6 || newFlag == ItemFlags.HiSlot7 || newFlag == ItemFlags.MedSlot0 ||
                newFlag == ItemFlags.MedSlot1 || newFlag == ItemFlags.MedSlot2 || newFlag == ItemFlags.MedSlot3 ||
                newFlag == ItemFlags.MedSlot4 || newFlag == ItemFlags.MedSlot5 || newFlag == ItemFlags.MedSlot6 ||
                newFlag == ItemFlags.MedSlot7 || newFlag == ItemFlags.LoSlot0 || newFlag == ItemFlags.LoSlot1 ||
                newFlag == ItemFlags.LoSlot2 || newFlag == ItemFlags.LoSlot3 || newFlag == ItemFlags.LoSlot4 ||
                newFlag == ItemFlags.LoSlot5 || newFlag == ItemFlags.LoSlot6 || newFlag == ItemFlags.LoSlot7)
            {
                ShipModule module = null;
                
                if (item.Quantity == 1)
                {
                    OnItemChange changes = new OnItemChange(item);

                    if (item.Singleton == false)
                        changes.AddChange(ItemChange.Singleton, item.Singleton);

                    item.LocationID = this.mInventory.ID;
                    item.Flag = newFlag;
                    item.Singleton = true;

                    changes
                        .AddChange(ItemChange.LocationID, oldLocation)
                        .AddChange(ItemChange.Flag, (int) oldFlag);
            
                    // notify the character about the change
                    Client.NotifyMultiEvent(changes);
                    // update meta inventories too
                    this.ItemManager.MetaInventoryManager.OnItemMoved(item, oldLocation, this.mInventory.ID);
            
                    // finally persist the item changes
                    item.Persist();

                    if (item is ShipModule shipModule)
                        module = shipModule;
                }
                else
                {
                    // item is not a singleton, create a new item, decrease quantity and send notifications
                    ItemEntity newItem = this.ItemManager.CreateSimpleItem(item.Type, item.OwnerID, this.mInventory.ID, newFlag, 1, false,
                        true);

                    item.Quantity -= 1;
                    
                    // notify the quantity change and the new item
                    Client.NotifyMultiEvent(OnItemChange.BuildQuantityChange(item, item.Quantity + 1));
                    Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(newItem, ItemFlags.None, 0));
                    
                    item.Persist();

                    if (newItem is ShipModule shipModule)
                        module = shipModule;
                }

                // put the module online after fitting it
                module?.PutOnline(Client);
                module?.Persist();
            }
            else
            {
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
        }

        public PyDataType Add(PyInteger itemID, CallInformation call)
        {
            if (itemID == call.Client.ShipID)
                throw new CantMoveActiveShip();

            ItemEntity item = this.ItemManager.GetItem(itemID);
            
            this.PreMoveItemCheck(item, this.mFlag, item.Quantity);
            this.MoveItemHere(item, this.mFlag);

            return null;
        }

        public PyDataType Add(PyInteger itemID, PyInteger quantity, CallInformation call)
        {
            if (itemID == call.Client.ShipID)
                throw new CantMoveActiveShip();

            ItemEntity item = this.ItemManager.GetItem(itemID);
            
            this.PreMoveItemCheck(item, this.mFlag, item.Quantity);
            this.MoveItemHere(item, this.mFlag);

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
            
            // check that there's enough space left
            this.PreMoveItemCheck(item, this.mFlag, quantity);

            // TODO: COPY THIS LOGIC OVER TO MOVEITEMHERE TO ENSURE THAT THIS ALSO FOLLOWS RULES REGARDING MODULES
            if (quantity == item.Quantity)
            {
                // the item is being moved completely, the easiest way is to remove from the old inventory
                // and put it in the new one
                this.MoveItemHere(item, (ItemFlags) (int) flag);
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

        public PyDataType MultiAdd(PyList adds, PyInteger quantity, PyInteger flag, CallInformation call)
        {
            if (quantity == null)
            {
                // null quantity means all the items in the list
                foreach (PyInteger itemID in adds.GetEnumerable<PyInteger>())
                {
                    ItemEntity item = this.ItemManager.GetItem(itemID);
                    
                    // check and then move the item
                    this.PreMoveItemCheck(item, (ItemFlags) (int) flag, item.Quantity);
                    this.MoveItemHere(item, (ItemFlags) (int) flag);
                }
            }
            else
            {
                // an specific quantity means we'll need to grab part of the stacks selected

                throw new CustomError("Not supported yet!");
            }
            
            return null;
        }

        public PyDataType MultiMerge(PyList merges, CallInformation call)
        {
            foreach (PyTuple merge in merges.GetEnumerable<PyTuple>())
            {
                if (merge[0] is PyInteger == false || merge[1] is PyInteger == false || merge[2] is PyInteger == false)
                    continue;

                PyInteger fromItemID = merge[0] as PyInteger;
                PyInteger toItemID = merge[1] as PyInteger;
                PyInteger quantity = merge[2] as PyInteger;

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

        public static PySubStruct BindInventory(ItemDB itemDB, ItemInventory item, ItemFlags flag, ItemManager itemManager, NodeContainer nodeContainer, BoundServiceManager boundServiceManager, Client client)
        {
            BoundService instance = new BoundInventory(itemDB, item, flag, itemManager, nodeContainer, boundServiceManager, client);
            // bind the service
            int boundID = boundServiceManager.BoundService(instance);
            // build the bound service string
            string boundServiceStr = boundServiceManager.BuildBoundServiceString(boundID);

            // TODO: the expiration time is 1 day, might be better to properly support this?
            // TODO: investigate these a bit more closely in the future
            // TODO: i'm not so sure about the expiration time
            PyTuple boundServiceInformation = new PyTuple(2)
            {
                [0] = boundServiceStr,
                [1] = DateTime.UtcNow.Add(TimeSpan.FromDays(1)).ToFileTime()
            };

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