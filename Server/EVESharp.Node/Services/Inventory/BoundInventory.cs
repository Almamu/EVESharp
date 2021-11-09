using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.corpRegistry;
using EVESharp.Node.Exceptions.inventory;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.StaticData.Corporation;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Exceptions;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Inventory
{
    public class BoundInventory : ClientBoundService
    {
        private ItemInventory mInventory;
        private Flags mFlag;
        private ItemDB ItemDB { get; }
        private NodeContainer NodeContainer { get; }
        private ItemFactory ItemFactory { get; }
        private NotificationManager NotificationManager { get; init; }

        public BoundInventory(ItemDB itemDB, ItemInventory item, ItemFactory itemFactory, NodeContainer nodeContainer, NotificationManager notificationManager, BoundServiceManager manager, Client client) : base(manager, client, item.ID)
        {
            this.mInventory = item;
            this.mFlag = Flags.None;
            this.ItemDB = itemDB;
            this.ItemFactory = itemFactory;
            this.NodeContainer = nodeContainer;
            this.NotificationManager = notificationManager;
        }

        public BoundInventory(ItemDB itemDB, ItemInventory item, Flags flag, ItemFactory itemFactory, NodeContainer nodeContainer, NotificationManager notificationManager, BoundServiceManager manager, Client client) : base(manager, client, item.ID)
        {
            this.mInventory = item;
            this.mFlag = flag;
            this.ItemDB = itemDB;
            this.ItemFactory = itemFactory;
            this.NodeContainer = nodeContainer;
            this.NotificationManager = notificationManager;
        }

        public PyDataType List(CallInformation call)
        {        
            CRowset result = new CRowset(ItemEntity.EntityItemDescriptor);

            foreach ((int _, ItemEntity item) in this.mInventory.Items)
                if (this.mFlag == Flags.None || item.Flag == this.mFlag)
                    result.Add(item.GetEntityRow());

            return result;
        }

        public PyDataType ListStations(PyInteger blueprintsOnly, PyInteger forCorp, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            return this.ItemDB.ListStations(forCorp == 1 ? call.Client.CorporationID : callerCharacterID, blueprintsOnly);
        }

        public PyDataType ListStationItems(PyInteger stationID, CallInformation call)
        {
            return this.ItemDB.ListStationItems(stationID, call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType ListStationBlueprintItems(PyInteger locationID, PyInteger _, PyInteger isCorp, CallInformation call)
        {
            if (isCorp == 1)
                return this.ItemDB.ListStationBlueprintItems(locationID, call.Client.CorporationID);

            return this.ItemDB.ListStationBlueprintItems(locationID, call.Client.EnsureCharacterIsSelected());
        }
        
        public PyDataType GetItem(CallInformation call)
        {
            return this.mInventory.GetEntityRow();
        }

        private void PreMoveItemCheck(ItemEntity item, Flags flag, double quantityToMove, Client client)
        {
            // check that where the item comes from we have permissions
            item.EnsureOwnership(client.EnsureCharacterIsSelected(), client.CorporationID, client.CorporationRole, true);
            
            if (this.mInventory.Type.ID == (int) Types.Capsule)
                throw new CantTakeInSpaceCapsule();
            // if this inventory is a delivery section, items cannot be moved to it
            if (this.mFlag == Flags.CorpMarket)
                throw new CustomError("You cannot move items into this hangar");

            // perform checks only on cargo
            if (this.mInventory is Ship ship && flag == Flags.Cargo)
            {
                // check destination cargo
                double currentVolume =
                    ship.Items.Sum(x => (x.Value.Flag != flag) ? 0.0 : x.Value.Quantity * x.Value.Attributes[Attributes.volume]);

                double newVolume = item.Attributes[Attributes.volume] * quantityToMove + currentVolume;
                double maxVolume = this.mInventory.Attributes[Attributes.capacity];

                if (newVolume > maxVolume)
                    throw new NotEnoughCargoSpace(currentVolume, this.mInventory.Attributes[Attributes.capacity] - currentVolume);
            }
        }

        private Flags GetFreeHighSlot(Ship ship)
        {
            Dictionary<Flags, ItemEntity> modules = ship.HighSlotModules;
            
            // ensure there's a free slot
            if (modules.Count >= ship.Attributes[Attributes.hiSlots])
                throw new NoFreeShipSlots();

            if (modules.ContainsKey(Flags.HiSlot0) == false)
                return Flags.HiSlot0;
            if (modules.ContainsKey(Flags.HiSlot1) == false)
                return Flags.HiSlot1;
            if (modules.ContainsKey(Flags.HiSlot2) == false)
                return Flags.HiSlot2;
            if (modules.ContainsKey(Flags.HiSlot3) == false)
                return Flags.HiSlot3;
            if (modules.ContainsKey(Flags.HiSlot4) == false)
                return Flags.HiSlot4;
            if (modules.ContainsKey(Flags.HiSlot5) == false)
                return Flags.HiSlot5;
            if (modules.ContainsKey(Flags.HiSlot6) == false)
                return Flags.HiSlot6;
            if (modules.ContainsKey(Flags.HiSlot7) == false)
                return Flags.HiSlot7;

            throw new NoFreeShipSlots();
        }

        private Flags GetFreeMediumSlot(Ship ship)
        {
            Dictionary<Flags, ItemEntity> modules = ship.MediumSlotModules;
            
            // ensure there's a free slot
            if (modules.Count >= ship.Attributes[Attributes.medSlots])
                throw new NoFreeShipSlots();

            if (modules.ContainsKey(Flags.MedSlot0) == false)
                return Flags.MedSlot0;
            if (modules.ContainsKey(Flags.MedSlot1) == false)
                return Flags.MedSlot1;
            if (modules.ContainsKey(Flags.MedSlot2) == false)
                return Flags.MedSlot2;
            if (modules.ContainsKey(Flags.MedSlot3) == false)
                return Flags.MedSlot3;
            if (modules.ContainsKey(Flags.MedSlot4) == false)
                return Flags.MedSlot4;
            if (modules.ContainsKey(Flags.MedSlot5) == false)
                return Flags.MedSlot5;
            if (modules.ContainsKey(Flags.MedSlot6) == false)
                return Flags.MedSlot6;
            if (modules.ContainsKey(Flags.MedSlot7) == false)
                return Flags.MedSlot7;

            throw new NoFreeShipSlots();
        }

        private Flags GetFreeLowSlot(Ship ship)
        {
            Dictionary<Flags, ItemEntity> modules = ship.LowSlotModules;
            
            // ensure there's a free slot
            if (modules.Count >= ship.Attributes[Attributes.lowSlots])
                throw new NoFreeShipSlots();

            if (modules.ContainsKey(Flags.LoSlot0) == false)
                return Flags.LoSlot0;
            if (modules.ContainsKey(Flags.LoSlot1) == false)
                return Flags.LoSlot1;
            if (modules.ContainsKey(Flags.LoSlot2) == false)
                return Flags.LoSlot2;
            if (modules.ContainsKey(Flags.LoSlot3) == false)
                return Flags.LoSlot3;
            if (modules.ContainsKey(Flags.LoSlot4) == false)
                return Flags.LoSlot4;
            if (modules.ContainsKey(Flags.LoSlot5) == false)
                return Flags.LoSlot5;
            if (modules.ContainsKey(Flags.LoSlot6) == false)
                return Flags.LoSlot6;
            if (modules.ContainsKey(Flags.LoSlot7) == false)
                return Flags.LoSlot7;

            throw new NoFreeShipSlots();
        }

        private Flags GetFreeRigSlot(Ship ship)
        {
            Dictionary<Flags, ItemEntity> modules = ship.RigSlots;
            
            // ensure there's a free slot
            if (modules.Count >= ship.Attributes[Attributes.rigSlots])
                throw new NoFreeShipSlots();

            if (modules.ContainsKey(Flags.RigSlot0) == false)
                return Flags.RigSlot0;
            if (modules.ContainsKey(Flags.RigSlot1) == false)
                return Flags.RigSlot1;
            if (modules.ContainsKey(Flags.RigSlot2) == false)
                return Flags.RigSlot2;
            if (modules.ContainsKey(Flags.RigSlot3) == false)
                return Flags.RigSlot3;
            if (modules.ContainsKey(Flags.RigSlot4) == false)
                return Flags.RigSlot4;
            if (modules.ContainsKey(Flags.RigSlot5) == false)
                return Flags.RigSlot5;
            if (modules.ContainsKey(Flags.RigSlot6) == false)
                return Flags.RigSlot6;
            if (modules.ContainsKey(Flags.RigSlot7) == false)
                return Flags.RigSlot7;

            throw new NoFreeShipSlots();
        }
        
        private void MoveItemHere(ItemEntity item, Flags newFlag, Client client, int quantity = 0)
        {
            // get the old location stored as it'll be used in the notifications
            int oldLocation = item.LocationID;
            Flags oldFlag = item.Flag;
            int oldOwnerID = item.OwnerID;
            
            // rig slots cannot be moved
            if (item.IsInRigSlot() == true)
                throw new CannotRemoveUpgradeManually();
            
            // special situation, if the old location is a module slot ensure the item is first offlined
            if (item.IsInModuleSlot() == true)
            {
                if (item is ShipModule module)
                {
                    if (module.Attributes[Attributes.isOnline] == 1)
                        module.StopApplyingEffect("online", Client);

                    // disable passive effects too
                    module.StopApplyingPassiveEffects(Client);
                }
            }
            
            // extra special situation, is the new flag an autofit one?
            if (newFlag == Flags.AutoFit)
            {
                // capsules cannot fit anything
                if (this.mInventory.Type.ID == (int) Types.Capsule)
                    throw new CantFitToCapsule();
                
                if (this.mInventory is Ship ship)
                {
                    // determine where to put the item
                    if (item is ShipModule module)
                    {
                        if (module.IsHighSlot() == true)
                            newFlag = this.GetFreeHighSlot(ship);
                        else if (module.IsMediumSlot() == true)
                            newFlag = this.GetFreeMediumSlot(ship);
                        else if (module.IsLowSlot() == true)
                            newFlag = this.GetFreeLowSlot(ship);
                        else if (module.IsRigSlot() == true)
                            newFlag = this.GetFreeRigSlot(ship);
                        else
                            // this item cannot be fitted, move it to cargo, maybe throw a exception about not being able to fit it?
                            newFlag = Flags.Cargo;
                    }
                    // TODO: HANDLE CHARGES!
                    else
                    {
                        newFlag = Flags.Cargo;
                    }
                }
                else
                {
                    newFlag = Flags.Hangar;
                }
            }

            // special case, is the item being moved to a corporation office?
            if (this.mInventory.OwnerID == client.CorporationID)
            {
                if (newFlag == Flags.Hangar && CorporationRole.HangarCanQuery1.Is(client.CorporationRole) == false)
                    throw new CrpAccessDenied("You are not allowed to access that hangar");
                if (newFlag == Flags.CorpSAG2 && CorporationRole.HangarCanQuery2.Is(client.CorporationRole) == false)
                    throw new CrpAccessDenied("You are not allowed to access that hangar");
                if (newFlag == Flags.CorpSAG3 && CorporationRole.HangarCanQuery3.Is(client.CorporationRole) == false)
                    throw new CrpAccessDenied("You are not allowed to access that hangar");
                if (newFlag == Flags.CorpSAG4 && CorporationRole.HangarCanQuery4.Is(client.CorporationRole) == false)
                    throw new CrpAccessDenied("You are not allowed to access that hangar");
                if (newFlag == Flags.CorpSAG5 && CorporationRole.HangarCanQuery5.Is(client.CorporationRole) == false)
                    throw new CrpAccessDenied("You are not allowed to access that hangar");
                if (newFlag == Flags.CorpSAG6 && CorporationRole.HangarCanQuery6.Is(client.CorporationRole) == false)
                    throw new CrpAccessDenied("You are not allowed to access that hangar");
                if (newFlag == Flags.CorpSAG7 && CorporationRole.HangarCanQuery7.Is(client.CorporationRole) == false)
                    throw new CrpAccessDenied("You are not allowed to access that hangar");
            }
            
            // special situation, if the new location is a module slot ensure the item is a singleton (TODO: HANDLE CHARGES TOO)
            if (newFlag.IsModule() == true)
            {
                ShipModule module = null;

                if (item is ShipModule shipModule)
                    module = shipModule;

                if (item.Quantity == 1)
                {
                    // remove item off the old inventory if required
                    if (this.ItemFactory.TryGetItem(item.LocationID, out ItemInventory inventory) == true)
                        inventory.RemoveItem(item);
                    
                    OnItemChange changes = new OnItemChange(item);

                    if (item.Singleton == false)
                        changes.AddChange(ItemChange.Singleton, item.Singleton);
                    if (item.OwnerID != this.mInventory.OwnerID)
                        changes.AddChange(ItemChange.OwnerID, item.OwnerID);

                    item.LocationID = this.mInventory.ID;
                    item.Flag = newFlag;
                    item.Singleton = true;
                    // update the owner ID to ensure the item stays visible (for example when moving from corp's deliveries to player's hangar)
                    item.OwnerID = this.mInventory.OwnerID;

                    changes
                        .AddChange(ItemChange.LocationID, oldLocation)
                        .AddChange(ItemChange.Flag, (int) oldFlag);
            
                    // notify the character about the change
                    this.NotificationManager.NotifyOwnerAtLocation(item.OwnerID, client.LocationID, changes);
                    // notify the old owner if it changed
                    if (item.OwnerID != oldOwnerID)
                        this.NotificationManager.NotifyOwnerAtLocation(oldOwnerID, client.LocationID, changes);
                    // update meta inventories too
                    this.ItemFactory.MetaInventoryManager.OnItemMoved(item, oldLocation, this.mInventory.ID, oldFlag, newFlag);

                    // finally persist the item changes
                    item.Persist();
                }
                else
                {
                    // item is not a singleton, create a new item, decrease quantity and send notifications
                    ItemEntity newItem = this.ItemFactory.CreateSimpleItem(item.Type, this.mInventory.OwnerID, this.mInventory.ID, newFlag, 1, false,
                        true);

                    item.Quantity -= 1;

                    OnItemChange quantityChange = OnItemChange.BuildQuantityChange(item, item.Quantity + 1);
                    // notify the character about the change in quantity
                    this.NotificationManager.NotifyOwnerAtLocation(item.OwnerID, client.LocationID, quantityChange);
                    // notify the old owner if it changed
                    if (item.OwnerID != oldOwnerID)
                        this.NotificationManager.NotifyOwnerAtLocation(item.OwnerID, client.LocationID, quantityChange);
                    
                    // notify the new owner on the new item
                    this.NotificationManager.NotifyOwnerAtLocation(item.OwnerID, client.LocationID, OnItemChange.BuildLocationChange(newItem, Flags.None, 0));
                    
                    item.Persist();

                    // replace reference so the following code handle things properly
                    item = newItem;
                    
                    if (item is ShipModule shipModule2)
                        module = shipModule2;
                }
                
                try
                {
                    // apply all the passive effects (this also blocks the item fitting if the initialization fails)
                    module?.ApplyPassiveEffects(Client);
                    // extra check, ensure that the character has the required skills
                }
                catch (UserError)
                {
                    // ensure that the passive effects that got applied already are removed from the item
                    module?.StopApplyingPassiveEffects(Client);

                    int newOldLocation = item.LocationID;
                    Flags newOldFlag = item.Flag;
                    int newOldOwnerID = item.OwnerID;
                    
                    // now undo the whole thing
                    item.LocationID = oldLocation;
                    item.Flag = oldFlag;
                    item.OwnerID = oldOwnerID;

                    OnItemChange locationChange = OnItemChange.BuildLocationChange(item, newOldFlag, newOldLocation);
                    
                    this.NotificationManager.NotifyOwnerAtLocation(item.OwnerID, client.LocationID, locationChange);
                    
                    // notify the owners again
                    if (item.OwnerID != newOldOwnerID)
                        this.NotificationManager.NotifyOwnerAtLocation(newOldOwnerID, client.LocationID, locationChange.AddChange (ItemChange.OwnerID, newOldOwnerID));
                    
                    throw;
                }

                // ensure the new inventory knows
                this.mInventory.AddItem(item);

                module?.Persist();
                
                // put the module online after fitting it as long as it's a normal module
                if (module?.IsRigSlot() == false)
                    module?.ApplyEffect("online", Client);
            }
            else
            {
                // zero quantity means move the whole stack
                if (quantity == 0 || item.Quantity == quantity)
                {
                    // remove item off the old inventory if required
                    if (this.ItemFactory.TryGetItem(item.LocationID, out ItemInventory inventory) == true)
                        inventory.RemoveItem(item);
                
                    OnItemChange changes = new OnItemChange(item);

                    if (item.OwnerID != this.mInventory.OwnerID)
                        changes.AddChange(ItemChange.OwnerID, item.OwnerID);

                    changes
                        .AddChange(ItemChange.LocationID, item.LocationID)
                        .AddChange(ItemChange.Flag, (int) item.Flag);
                
                    // set the new location for the item
                    item.LocationID = this.mInventory.ID;
                    item.Flag = newFlag;
                    item.OwnerID = this.mInventory.OwnerID;

                    // notify the old owner
                    if (oldOwnerID != item.OwnerID)
                        this.NotificationManager.NotifyOwnerAtLocation(oldOwnerID, client.LocationID, changes);
                
                    // notify the new owner
                    this.NotificationManager.NotifyOwnerAtLocation(item.OwnerID, client.LocationID, changes);
                    // update meta inventories too
                    this.ItemFactory.MetaInventoryManager.OnItemMoved(item, oldLocation, this.mInventory.ID, oldFlag, newFlag);

                    // ensure the new inventory knows
                    this.mInventory.AddItem(item);
                }
                else
                {
                    // a specified quantity means move only that quantity, so the easiest way is to decrement the stack
                    // and create a new item in the correct place
                    // item is not a singleton, create a new item, decrease quantity and send notifications
                    ItemEntity newItem = this.ItemFactory.CreateSimpleItem(item.Type, this.mInventory.OwnerID, this.mInventory.ID, newFlag, quantity, item.Contraband, item.Singleton);

                    item.Quantity -= quantity;

                    OnItemChange quantityChange = OnItemChange.BuildQuantityChange(item, item.Quantity + quantity);
                    // notify the character about the change in quantity
                    this.NotificationManager.NotifyOwnerAtLocation(item.OwnerID, client.LocationID, quantityChange);
                    // notify the old owner if it changed
                    if (item.OwnerID != oldOwnerID)
                        this.NotificationManager.NotifyOwnerAtLocation(item.OwnerID, client.LocationID, quantityChange);
                    
                    // notify the new owner on the new item
                    this.NotificationManager.NotifyOwnerAtLocation(item.OwnerID, client.LocationID, OnItemChange.BuildLocationChange(newItem, Flags.None, 0));
                    
                    item.Persist();

                    // replace reference so the following code handle things properly
                    item = newItem;
                }

                // finally persist the item changes
                item.Persist();
            }
        }

        public PyDataType Add(PyInteger itemID, CallInformation call)
        {
            if (itemID == call.Client.ShipID)
                throw new CantMoveActiveShip();

            ItemEntity item = this.ItemFactory.GetItem(itemID);
            
            this.PreMoveItemCheck(item, this.mFlag, item.Quantity, call.Client);
            this.MoveItemHere(item, this.mFlag, call.Client);

            return null;
        }

        public PyDataType Add(PyInteger itemID, PyInteger quantity, CallInformation call)
        {
            if (itemID == call.Client.ShipID)
                throw new CantMoveActiveShip();

            ItemEntity item = this.ItemFactory.GetItem(itemID);
            
            this.PreMoveItemCheck(item, this.mFlag, item.Quantity, call.Client);
            this.MoveItemHere(item, this.mFlag, call.Client, quantity);

            return null;
        }

        public PyDataType Add(PyInteger itemID, PyInteger quantity, PyInteger flag, CallInformation call)
        {
            if (itemID == call.Client.ShipID)
                throw new CantMoveActiveShip();

            // TODO: ADD CONSTRAINTS CHECKS FOR THE FLAG
            ItemEntity item = this.ItemFactory.GetItem(itemID);

            // ensure there's enough quantity in the stack to split it
            if (quantity > item.Quantity)
                return null;
            
            // check that there's enough space left
            this.PreMoveItemCheck(item, (Flags) (int) flag, quantity, call.Client);
            this.MoveItemHere(item, (Flags) (int) flag, call.Client, quantity);
            
            return null;
        }

        public PyDataType MultiAdd(PyList adds, PyInteger quantity, PyInteger flag, CallInformation call)
        {
            if (quantity == null)
            {
                // null quantity means all the items in the list
                foreach (PyInteger itemID in adds.GetEnumerable<PyInteger>())
                {
                    ItemEntity item = this.ItemFactory.GetItem(itemID);
                    
                    // check and then move the item
                    this.PreMoveItemCheck(item, (Flags) (int) flag, item.Quantity, call.Client);
                    this.MoveItemHere(item, (Flags) (int) flag, call.Client);
                }
            }
            else
            {
                // an specific quantity means we'll need to grab part of the stacks selected

                throw new CustomError("Not supported yet!");
            }
            
            return null;
        }

        public PyDataType MultiAdd(PyList adds, CallInformation call)
        {
            return this.MultiAdd(adds, null, (int) this.mFlag, call);
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

                ItemEntity fromItem = this.ItemFactory.GetItem(fromItemID);

                // ignore singleton items
                if (fromItem.Singleton == true || toItem.Singleton == true)
                    continue;
                // ignore items that are not the same type
                if (fromItem.Type.ID != toItem.Type.ID)
                    continue;
                // if we're fully merging two stacks, just remove one item
                if (quantity == fromItem.Quantity)
                {
                    int oldLocationID = fromItem.LocationID;
                    // remove the item
                    this.ItemFactory.DestroyItem(fromItem);
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

        private void StackAll(Flags locationFlag, CallInformation call)
        {
            // TODO: ADD CONSTRAINTS CHECKS FOR THE LOCATIONFLAG
            foreach ((int firstItemID, ItemEntity firstItem) in this.mInventory.Items)
            {
                // singleton items are not even checked
                if (firstItem.Singleton == true || firstItem.Flag != locationFlag)
                    continue;
                
                foreach ((int secondItemID, ItemEntity secondItem) in this.mInventory.Items)
                {
                    // ignore the same itemID as they cannot really be merged
                    if (firstItemID == secondItemID)
                        continue;
                    // ignore the item if it's singleton
                    if (secondItem.Singleton == true || secondItem.Flag != locationFlag)
                        continue;
                    // ignore the item check if they're not the same type ID
                    if (firstItem.Type.ID != secondItem.Type.ID)
                        continue;
                    int oldQuantity = secondItem.Quantity;
                    // add the quantity of the first item to the second
                    secondItem.Quantity += firstItem.Quantity;
                    // also create the notification for the user
                    call.Client.NotifyMultiEvent(OnItemChange.BuildQuantityChange(secondItem, oldQuantity));
                    this.ItemFactory.DestroyItem(firstItem);
                    // notify the client about the item too
                    call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(firstItem, firstItem.Flag, secondItem.LocationID));
                    // ensure the second item is saved to database too
                    secondItem.Persist();
                    // finally break this loop as the merge was already done
                    break;
                }
            }
        }

        public PyDataType StackAll(PyInteger locationFlag, CallInformation call)
        {
            if (this.mFlag != Flags.None)
                return null;
            
            this.StackAll((Flags) (int) locationFlag, call);

            return null;
        }
        
        public PyDataType StackAll(CallInformation call)
        {
            if (this.mFlag == Flags.None)
                return null;
            
            this.StackAll(this.mFlag, call);
            
            return null;
        }

        public static PySubStruct BindInventory(ItemDB itemDB, ItemInventory item, Flags flag, ItemFactory itemFactory, NodeContainer nodeContainer, NotificationManager notificationManager, BoundServiceManager boundServiceManager, Client client)
        {
            BoundService instance = new BoundInventory(itemDB, item, flag, itemFactory, nodeContainer, notificationManager, boundServiceManager, client);
            // bind the service
            int boundID = boundServiceManager.BindService(instance);
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

        public PyDataType DestroyFitting(PyInteger itemID, CallInformation call)
        {
            ItemEntity item = this.mInventory.Items[itemID];

            if (item.IsInRigSlot() == false)
                throw new CannotDestroyFittedItem();

            if (item is ShipModule module)
            {
                // disable passive effects
                module.StopApplyingPassiveEffects(Client);
            }
            
            int oldLocationID = item.LocationID;
            Flags oldFlag = item.Flag;
            
            // destroy the rig
            this.ItemFactory.DestroyItem(item);

            // notify the client about the change
            call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(item, oldFlag, oldLocationID));
            
            return null;
        }

        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            throw new NotImplementedException();
        }

        protected override BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            throw new NotImplementedException();
        }
    }
}