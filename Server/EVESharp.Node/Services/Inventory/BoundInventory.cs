using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Dogma;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Exceptions.inventory;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types;
using EVESharp.Node.Database;
using EVESharp.Node.Dogma;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Inventory;

public class BoundInventory : ClientBoundService
{
    private readonly Flags mFlag;

    private readonly ItemInventory       mInventory;
    public override  AccessLevel         AccessLevel        => AccessLevel.None;
    private          ItemDB              ItemDB             { get; }
    private          IItems              Items              { get; }
    private          INotificationSender Notifications      { get; }
    private          IDogmaNotifications DogmaNotifications { get; }
    private          EffectsManager      EffectsManager     { get; }

    public BoundInventory
    (
        ItemDB              itemDB,             EffectsManager      effectsManager,     ItemInventory       item,    IItems  items,
        INotificationSender notificationSender, IDogmaNotifications dogmaNotifications, BoundServiceManager manager, Session session
    ) : base (manager, session, item.ID)
    {
        EffectsManager     = effectsManager;
        this.mInventory    = item;
        this.mFlag         = Flags.None;
        ItemDB             = itemDB;
        Items              = items;
        Notifications      = notificationSender;
        DogmaNotifications = dogmaNotifications;
    }

    public BoundInventory
    (
        ItemDB              itemDB,             EffectsManager      effectsManager,     ItemInventory       item,    Flags   flag, IItems items,
        INotificationSender notificationSender, IDogmaNotifications dogmaNotifications, BoundServiceManager manager, Session session
    ) : base (manager, session, item.ID)
    {
        EffectsManager     = effectsManager;
        this.mInventory    = item;
        this.mFlag         = flag;
        ItemDB             = itemDB;
        Items              = items;
        Notifications      = notificationSender;
        DogmaNotifications = dogmaNotifications;
    }

    public PyDataType List (CallInformation call)
    {
        CRowset result = new CRowset (ItemEntity.EntityItemDescriptor);

        foreach ((int _, ItemEntity item) in this.mInventory.Items)
            if (this.mFlag == Flags.None || item.Flag == this.mFlag)
                result.Add (item.GetEntityRow ());

        return result;
    }

    public PyDataType ListStations (CallInformation call, PyInteger blueprintsOnly, PyInteger forCorp)
    {
        int callerCharacterID = call.Session.CharacterID;

        return ItemDB.ListStations (forCorp == 1 ? call.Session.CorporationID : callerCharacterID, blueprintsOnly);
    }

    public PyDataType ListStationItems (CallInformation call, PyInteger stationID)
    {
        return ItemDB.ListStationItems (stationID, call.Session.CharacterID);
    }

    public PyDataType ListStationBlueprintItems (CallInformation call, PyInteger locationID, PyInteger _, PyInteger isCorp)
    {
        if (isCorp == 1)
            return ItemDB.ListStationBlueprintItems (locationID, call.Session.CorporationID);

        return ItemDB.ListStationBlueprintItems (locationID, call.Session.CharacterID);
    }

    public PyDataType GetItem (CallInformation call)
    {
        return this.mInventory.GetEntityRow ();
    }

    private void PreMoveItemCheck (ItemEntity item, Flags flag, double quantityToMove, Session session)
    {
        // check that where the item comes from we have permissions
        item.EnsureOwnership (session.CharacterID, session.CorporationID, session.CorporationRole, true);

        if (this.mInventory.Type.ID == (int) TypeID.Capsule)
            throw new CantTakeInSpaceCapsule ();

        // if this inventory is a delivery section, items cannot be moved to it
        if (this.mFlag == Flags.CorpMarket)
            throw new CustomError ("You cannot move items into this hangar");

        // perform checks only on cargo
        if (this.mInventory is Ship ship && flag == Flags.Cargo)
        {
            // check destination cargo
            double currentVolume =
                ship.Items.Sum (x => x.Value.Flag != flag ? 0.0 : x.Value.Quantity * x.Value.Attributes [AttributeTypes.volume]);

            double newVolume = item.Attributes [AttributeTypes.volume] * quantityToMove + currentVolume;
            double maxVolume = this.mInventory.Attributes [AttributeTypes.capacity];

            if (newVolume > maxVolume)
                throw new NotEnoughCargoSpace (currentVolume, this.mInventory.Attributes [AttributeTypes.capacity] - currentVolume);
        }
    }

    private Flags GetFreeHighSlot (Ship ship)
    {
        Dictionary <Flags, ItemEntity> modules = ship.HighSlotModules;

        // ensure there's a free slot
        if (modules.Count >= ship.Attributes [AttributeTypes.hiSlots])
            throw new NoFreeShipSlots ();

        if (modules.ContainsKey (Flags.HiSlot0) == false)
            return Flags.HiSlot0;

        if (modules.ContainsKey (Flags.HiSlot1) == false)
            return Flags.HiSlot1;

        if (modules.ContainsKey (Flags.HiSlot2) == false)
            return Flags.HiSlot2;

        if (modules.ContainsKey (Flags.HiSlot3) == false)
            return Flags.HiSlot3;

        if (modules.ContainsKey (Flags.HiSlot4) == false)
            return Flags.HiSlot4;

        if (modules.ContainsKey (Flags.HiSlot5) == false)
            return Flags.HiSlot5;

        if (modules.ContainsKey (Flags.HiSlot6) == false)
            return Flags.HiSlot6;

        if (modules.ContainsKey (Flags.HiSlot7) == false)
            return Flags.HiSlot7;

        throw new NoFreeShipSlots ();
    }

    private Flags GetFreeMediumSlot (Ship ship)
    {
        Dictionary <Flags, ItemEntity> modules = ship.MediumSlotModules;

        // ensure there's a free slot
        if (modules.Count >= ship.Attributes [AttributeTypes.medSlots])
            throw new NoFreeShipSlots ();

        if (modules.ContainsKey (Flags.MedSlot0) == false)
            return Flags.MedSlot0;

        if (modules.ContainsKey (Flags.MedSlot1) == false)
            return Flags.MedSlot1;

        if (modules.ContainsKey (Flags.MedSlot2) == false)
            return Flags.MedSlot2;

        if (modules.ContainsKey (Flags.MedSlot3) == false)
            return Flags.MedSlot3;

        if (modules.ContainsKey (Flags.MedSlot4) == false)
            return Flags.MedSlot4;

        if (modules.ContainsKey (Flags.MedSlot5) == false)
            return Flags.MedSlot5;

        if (modules.ContainsKey (Flags.MedSlot6) == false)
            return Flags.MedSlot6;

        if (modules.ContainsKey (Flags.MedSlot7) == false)
            return Flags.MedSlot7;

        throw new NoFreeShipSlots ();
    }

    private Flags GetFreeLowSlot (Ship ship)
    {
        Dictionary <Flags, ItemEntity> modules = ship.LowSlotModules;

        // ensure there's a free slot
        if (modules.Count >= ship.Attributes [AttributeTypes.lowSlots])
            throw new NoFreeShipSlots ();

        if (modules.ContainsKey (Flags.LoSlot0) == false)
            return Flags.LoSlot0;

        if (modules.ContainsKey (Flags.LoSlot1) == false)
            return Flags.LoSlot1;

        if (modules.ContainsKey (Flags.LoSlot2) == false)
            return Flags.LoSlot2;

        if (modules.ContainsKey (Flags.LoSlot3) == false)
            return Flags.LoSlot3;

        if (modules.ContainsKey (Flags.LoSlot4) == false)
            return Flags.LoSlot4;

        if (modules.ContainsKey (Flags.LoSlot5) == false)
            return Flags.LoSlot5;

        if (modules.ContainsKey (Flags.LoSlot6) == false)
            return Flags.LoSlot6;

        if (modules.ContainsKey (Flags.LoSlot7) == false)
            return Flags.LoSlot7;

        throw new NoFreeShipSlots ();
    }

    private Flags GetFreeRigSlot (Ship ship)
    {
        Dictionary <Flags, ItemEntity> modules = ship.RigSlots;

        // ensure there's a free slot
        if (modules.Count >= ship.Attributes [AttributeTypes.rigSlots])
            throw new NoFreeShipSlots ();

        if (modules.ContainsKey (Flags.RigSlot0) == false)
            return Flags.RigSlot0;

        if (modules.ContainsKey (Flags.RigSlot1) == false)
            return Flags.RigSlot1;

        if (modules.ContainsKey (Flags.RigSlot2) == false)
            return Flags.RigSlot2;

        if (modules.ContainsKey (Flags.RigSlot3) == false)
            return Flags.RigSlot3;

        if (modules.ContainsKey (Flags.RigSlot4) == false)
            return Flags.RigSlot4;

        if (modules.ContainsKey (Flags.RigSlot5) == false)
            return Flags.RigSlot5;

        if (modules.ContainsKey (Flags.RigSlot6) == false)
            return Flags.RigSlot6;

        if (modules.ContainsKey (Flags.RigSlot7) == false)
            return Flags.RigSlot7;

        throw new NoFreeShipSlots ();
    }

    private void MoveItemHere (ItemEntity item, Flags newFlag, Session session, int quantity = 0)
    {
        // get the old location stored as it'll be used in the notifications
        int   oldLocation = item.LocationID;
        Flags oldFlag     = item.Flag;
        int   oldOwnerID  = item.OwnerID;

        // rig slots cannot be moved
        if (item.IsInRigSlot ())
            throw new CannotRemoveUpgradeManually ();

        // special situation, if the old location is a module slot ensure the item is first offlined
        if (item.IsInModuleSlot ())
            if (item is ShipModule module)
            {
                ItemEffects effects = EffectsManager.GetForItem (module, session);

                if (module.Attributes [AttributeTypes.isOnline] == 1)
                    effects.StopApplyingEffect ("online", session);

                // disable passive effects too
                effects.StopApplyingPassiveEffects (session);
            }

        // extra special situation, is the new flag an autofit one?
        if (newFlag == Flags.AutoFit)
        {
            // capsules cannot fit anything
            if (this.mInventory.Type.ID == (int) TypeID.Capsule)
                throw new CantFitToCapsule ();

            if (this.mInventory is Ship ship)
            {
                // determine where to put the item
                if (item is ShipModule module)
                {
                    if (module.IsHighSlot ())
                        newFlag = this.GetFreeHighSlot (ship);
                    else if (module.IsMediumSlot ())
                        newFlag = this.GetFreeMediumSlot (ship);
                    else if (module.IsLowSlot ())
                        newFlag = this.GetFreeLowSlot (ship);
                    else if (module.IsRigSlot ())
                        newFlag = this.GetFreeRigSlot (ship);
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
        if (this.mInventory.OwnerID == session.CorporationID)
        {
            if (newFlag == Flags.Hangar && CorporationRole.HangarCanQuery1.Is (session.CorporationRole) == false)
                throw new CrpAccessDenied ("You are not allowed to access that hangar");

            if (newFlag == Flags.CorpSAG2 && CorporationRole.HangarCanQuery2.Is (session.CorporationRole) == false)
                throw new CrpAccessDenied ("You are not allowed to access that hangar");

            if (newFlag == Flags.CorpSAG3 && CorporationRole.HangarCanQuery3.Is (session.CorporationRole) == false)
                throw new CrpAccessDenied ("You are not allowed to access that hangar");

            if (newFlag == Flags.CorpSAG4 && CorporationRole.HangarCanQuery4.Is (session.CorporationRole) == false)
                throw new CrpAccessDenied ("You are not allowed to access that hangar");

            if (newFlag == Flags.CorpSAG5 && CorporationRole.HangarCanQuery5.Is (session.CorporationRole) == false)
                throw new CrpAccessDenied ("You are not allowed to access that hangar");

            if (newFlag == Flags.CorpSAG6 && CorporationRole.HangarCanQuery6.Is (session.CorporationRole) == false)
                throw new CrpAccessDenied ("You are not allowed to access that hangar");

            if (newFlag == Flags.CorpSAG7 && CorporationRole.HangarCanQuery7.Is (session.CorporationRole) == false)
                throw new CrpAccessDenied ("You are not allowed to access that hangar");
        }

        // special situation, if the new location is a module slot ensure the item is a singleton (TODO: HANDLE CHARGES TOO)
        if (newFlag.IsModule ())
        {
            ShipModule module = null;

            if (item is ShipModule shipModule)
                module = shipModule;

            if (item.Quantity == 1)
            {
                // remove item off the old inventory if required
                if (this.Items.TryGetItem (item.LocationID, out ItemInventory inventory))
                    inventory.RemoveItem (item);

                OnItemChange changes = new OnItemChange (item);

                if (item.Singleton == false)
                    changes.AddChange (ItemChange.Singleton, item.Singleton);

                if (item.OwnerID != this.mInventory.OwnerID)
                    changes.AddChange (ItemChange.OwnerID, item.OwnerID);

                item.LocationID = this.mInventory.ID;
                item.Flag       = newFlag;
                item.Singleton  = true;
                // update the owner ID to ensure the item stays visible (for example when moving from corp's deliveries to player's hangar)
                item.OwnerID = this.mInventory.OwnerID;

                changes
                    .AddChange (ItemChange.LocationID, oldLocation)
                    .AddChange (ItemChange.Flag,       (int) oldFlag);

                // notify the character about the change
                Notifications.NotifyOwnerAtLocation (item.OwnerID, session.LocationID, changes);

                // notify the old owner if it changed
                if (item.OwnerID != oldOwnerID)
                    Notifications.NotifyOwnerAtLocation (oldOwnerID, session.LocationID, changes);

                // update meta inventories too
                this.Items.MetaInventories.OnItemMoved (item, oldLocation, this.mInventory.ID, oldFlag, newFlag);

                // finally persist the item changes
                item.Persist ();
            }
            else
            {
                // item is not a singleton, create a new item, decrease quantity and send notifications
                ItemEntity newItem = this.Items.CreateSimpleItem (
                    item.Type, this.mInventory.OwnerID, this.mInventory.ID, newFlag, 1, false,
                    true
                );

                item.Quantity -= 1;

                OnItemChange quantityChange = OnItemChange.BuildQuantityChange (item, item.Quantity + 1);
                // notify the character about the change in quantity
                Notifications.NotifyOwnerAtLocation (item.OwnerID, session.LocationID, quantityChange);

                // notify the old owner if it changed
                if (item.OwnerID != oldOwnerID)
                    Notifications.NotifyOwnerAtLocation (item.OwnerID, session.LocationID, quantityChange);

                // notify the new owner on the new item
                Notifications.NotifyOwnerAtLocation (item.OwnerID, session.LocationID, OnItemChange.BuildLocationChange (newItem, Flags.None, 0));

                item.Persist ();

                // replace reference so the following code handle things properly
                item = newItem;

                if (item is ShipModule shipModule2)
                    module = shipModule2;
            }

            ItemEffects effects = EffectsManager.GetForItem (module, session);

            try
            {
                // apply all the passive effects (this also blocks the item fitting if the initialization fails)
                effects?.ApplyPassiveEffects (session);
                // extra check, ensure that the character has the required skills
            }
            catch (UserError)
            {
                // ensure that the passive effects that got applied already are removed from the item
                effects?.StopApplyingPassiveEffects (session);

                int   newOldLocation = item.LocationID;
                Flags newOldFlag     = item.Flag;
                int   newOldOwnerID  = item.OwnerID;

                // now undo the whole thing
                item.LocationID = oldLocation;
                item.Flag       = oldFlag;
                item.OwnerID    = oldOwnerID;

                OnItemChange locationChange = OnItemChange.BuildLocationChange (item, newOldFlag, newOldLocation);

                Notifications.NotifyOwnerAtLocation (item.OwnerID, session.LocationID, locationChange);

                // notify the owners again
                if (item.OwnerID != newOldOwnerID)
                    Notifications.NotifyOwnerAtLocation (newOldOwnerID, session.LocationID, locationChange.AddChange (ItemChange.OwnerID, newOldOwnerID));

                throw;
            }

            // ensure the new inventory knows
            this.mInventory.AddItem (item);

            module?.Persist ();

            // put the module online after fitting it as long as it's a normal module
            if (module?.IsRigSlot () == false)
                effects?.ApplyEffect ("online", session);
        }
        else
        {
            // zero quantity means move the whole stack
            if (quantity == 0 || item.Quantity == quantity)
            {
                // remove item off the old inventory if required
                if (this.Items.TryGetItem (item.LocationID, out ItemInventory inventory))
                    inventory.RemoveItem (item);

                OnItemChange changes = new OnItemChange (item);

                if (item.OwnerID != this.mInventory.OwnerID)
                    changes.AddChange (ItemChange.OwnerID, item.OwnerID);

                changes
                    .AddChange (ItemChange.LocationID, item.LocationID)
                    .AddChange (ItemChange.Flag,       (int) item.Flag);

                // set the new location for the item
                item.LocationID = this.mInventory.ID;
                item.Flag       = newFlag;
                item.OwnerID    = this.mInventory.OwnerID;

                // notify the old owner
                if (oldOwnerID != item.OwnerID)
                    Notifications.NotifyOwnerAtLocation (oldOwnerID, session.LocationID, changes);

                // notify the new owner
                Notifications.NotifyOwnerAtLocation (item.OwnerID, session.LocationID, changes);
                // update meta inventories too
                this.Items.MetaInventories.OnItemMoved (item, oldLocation, this.mInventory.ID, oldFlag, newFlag);

                // ensure the new inventory knows
                this.mInventory.AddItem (item);
            }
            else
            {
                // a specified quantity means move only that quantity, so the easiest way is to decrement the stack
                // and create a new item in the correct place
                // item is not a singleton, create a new item, decrease quantity and send notifications
                ItemEntity newItem = this.Items.CreateSimpleItem (
                    item.Type, this.mInventory.OwnerID, this.mInventory.ID, newFlag, quantity, item.Contraband, item.Singleton
                );

                item.Quantity -= quantity;

                OnItemChange quantityChange = OnItemChange.BuildQuantityChange (item, item.Quantity + quantity);
                // notify the character about the change in quantity
                Notifications.NotifyOwnerAtLocation (item.OwnerID, session.LocationID, quantityChange);

                // notify the old owner if it changed
                if (item.OwnerID != oldOwnerID)
                    Notifications.NotifyOwnerAtLocation (item.OwnerID, session.LocationID, quantityChange);

                // notify the new owner on the new item
                Notifications.NotifyOwnerAtLocation (item.OwnerID, session.LocationID, OnItemChange.BuildLocationChange (newItem, Flags.None, 0));

                item.Persist ();

                // replace reference so the following code handle things properly
                item = newItem;
            }

            // finally persist the item changes
            item.Persist ();
        }
    }

    public PyDataType Add (CallInformation call, PyInteger itemID)
    {
        if (itemID == call.Session.ShipID)
            throw new CantMoveActiveShip ();

        ItemEntity item = this.Items.GetItem (itemID);

        this.PreMoveItemCheck (item, this.mFlag, item.Quantity, call.Session);
        this.MoveItemHere (item, this.mFlag, call.Session);

        return null;
    }

    public PyDataType Add (CallInformation call, PyInteger itemID, PyInteger quantity)
    {
        if (itemID == call.Session.ShipID)
            throw new CantMoveActiveShip ();

        ItemEntity item = this.Items.GetItem (itemID);

        this.PreMoveItemCheck (item, this.mFlag, item.Quantity, call.Session);
        this.MoveItemHere (item, this.mFlag, call.Session, quantity);

        return null;
    }

    public PyDataType Add (CallInformation call, PyInteger itemID, PyInteger quantity, PyInteger flag)
    {
        if (itemID == call.Session.ShipID)
            throw new CantMoveActiveShip ();

        // TODO: ADD CONSTRAINTS CHECKS FOR THE FLAG
        ItemEntity item = this.Items.GetItem (itemID);

        // ensure there's enough quantity in the stack to split it
        if (quantity > item.Quantity)
            return null;

        // check that there's enough space left
        this.PreMoveItemCheck (item, (Flags) (int) flag, quantity, call.Session);
        this.MoveItemHere (item, (Flags) (int) flag, call.Session, quantity);

        return null;
    }

    public PyDataType MultiAdd (CallInformation call, PyList adds, PyInteger quantity, PyInteger flag)
    {
        if (quantity == null)
            // null quantity means all the items in the list
            foreach (PyInteger itemID in adds.GetEnumerable <PyInteger> ())
            {
                ItemEntity item = this.Items.GetItem (itemID);

                // check and then move the item
                this.PreMoveItemCheck (item, (Flags) (int) flag, item.Quantity, call.Session);
                this.MoveItemHere (item, (Flags) (int) flag, call.Session);
            }
        else
            // an specific quantity means we'll need to grab part of the stacks selected

            throw new CustomError ("Not supported yet!");

        return null;
    }

    public PyDataType MultiAdd (CallInformation call, PyList adds)
    {
        return this.MultiAdd (call, adds, null, (int) this.mFlag);
    }

    public PyDataType MultiMerge (CallInformation call, PyList merges)
    {
        int callerCharacterID = call.Session.CharacterID;

        foreach (PyTuple merge in merges.GetEnumerable <PyTuple> ())
        {
            if (merge [0] is PyInteger == false || merge [1] is PyInteger == false || merge [2] is PyInteger == false)
                continue;

            PyInteger fromItemID = merge [0] as PyInteger;
            PyInteger toItemID   = merge [1] as PyInteger;
            PyInteger quantity   = merge [2] as PyInteger;

            if (this.mInventory.Items.TryGetValue (toItemID, out ItemEntity toItem) == false)
                continue;

            ItemEntity fromItem = this.Items.GetItem (fromItemID);

            // ignore singleton items
            if (fromItem.Singleton || toItem.Singleton)
                continue;

            // ignore items that are not the same type
            if (fromItem.Type.ID != toItem.Type.ID)
                continue;

            // if we're fully merging two stacks, just remove one item
            if (quantity == fromItem.Quantity)
            {
                int oldLocationID = fromItem.LocationID;
                // remove the item
                this.Items.DestroyItem (fromItem);
                // notify the client about the item too
                this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildLocationChange (fromItem, oldLocationID));
            }
            else
            {
                // change the item's quantity
                fromItem.Quantity -= quantity;
                // notify the client about the change
                this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildQuantityChange (fromItem, fromItem.Quantity + quantity));
                fromItem.Persist ();
            }

            toItem.Quantity += quantity;
            this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildQuantityChange (toItem, toItem.Quantity - quantity));
            toItem.Persist ();
        }

        return null;
    }

    public PyDataType StackAll (CallInformation call, PyString password)
    {
        throw new NotImplementedException ("Stacking on passworded containers is not supported yet!");
    }

    private void StackAll (CallInformation call, Flags locationFlag)
    {
        int callerCharacterID = call.Session.CharacterID;

        // TODO: ADD CONSTRAINTS CHECKS FOR THE LOCATIONFLAG
        foreach ((int firstItemID, ItemEntity firstItem) in this.mInventory.Items)
        {
            // singleton items are not even checked
            if (firstItem.Singleton || firstItem.Flag != locationFlag)
                continue;

            foreach ((int secondItemID, ItemEntity secondItem) in this.mInventory.Items)
            {
                // ignore the same itemID as they cannot really be merged
                if (firstItemID == secondItemID)
                    continue;

                // ignore the item if it's singleton
                if (secondItem.Singleton || secondItem.Flag != locationFlag)
                    continue;

                // ignore the item check if they're not the same type ID
                if (firstItem.Type.ID != secondItem.Type.ID)
                    continue;

                int oldQuantity = secondItem.Quantity;
                // add the quantity of the first item to the second
                secondItem.Quantity += firstItem.Quantity;
                // also create the notification for the user
                this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildQuantityChange (secondItem, oldQuantity));
                this.Items.DestroyItem (firstItem);

                // notify the client about the item too
                this.DogmaNotifications.QueueMultiEvent (
                    callerCharacterID, OnItemChange.BuildLocationChange (firstItem, firstItem.Flag, secondItem.LocationID)
                );

                // ensure the second item is saved to database too
                secondItem.Persist ();

                // finally break this loop as the merge was already done
                break;
            }
        }
    }

    public PyDataType StackAll (CallInformation call, PyInteger locationFlag)
    {
        if (this.mFlag != Flags.None)
            return null;

        this.StackAll (call, (Flags) (int) locationFlag);

        return null;
    }

    public PyDataType StackAll (CallInformation call)
    {
        if (this.mFlag == Flags.None)
            return null;

        this.StackAll (call, this.mFlag);

        return null;
    }

    public static PySubStruct BindInventory
    (
        ItemDB              itemDB,             EffectsManager      effectsManager,     ItemInventory       item,                Flags   flag, IItems items,
        INotificationSender notificationSender, IDogmaNotifications dogmaNotifications, BoundServiceManager boundServiceManager, Session session
    )
    {
        BoundService instance = new BoundInventory (
            itemDB, effectsManager, item, flag, items, notificationSender, dogmaNotifications,
            boundServiceManager, session
        );

        // bind the service
        int boundID = boundServiceManager.BindService (instance);
        // build the bound service string
        string boundServiceStr = boundServiceManager.BuildBoundServiceString (boundID);

        // TODO: the expiration time is 1 day, might be better to properly support this?
        // TODO: investigate these a bit more closely in the future
        // TODO: i'm not so sure about the expiration time
        PyTuple boundServiceInformation = new PyTuple (2)
        {
            [0] = boundServiceStr,
            [1] = DateTime.UtcNow.Add (TimeSpan.FromDays (1)).ToFileTime ()
        };

        // after the service is bound the call can be run (if required)
        return new PySubStruct (new PySubStream (boundServiceInformation));
    }

    public PyDataType BreakPlasticWrap (CallInformation call, PyInteger crateID)
    {
        // TODO: ensure this item is a plastic wrap and mark the contract as void
        return null;
    }

    public PyDataType DestroyFitting (CallInformation call, PyInteger itemID)
    {
        ItemEntity item = this.mInventory.Items [itemID];

        if (item.IsInRigSlot () == false)
            throw new CannotDestroyFittedItem ();

        if (item is ShipModule module)
            // disable passive effects
            EffectsManager.GetForItem (module, call.Session).StopApplyingPassiveEffects (call.Session);

        int   oldLocationID = item.LocationID;
        Flags oldFlag       = item.Flag;

        // destroy the rig
        this.Items.DestroyItem (item);

        // notify the client about the change
        this.DogmaNotifications.QueueMultiEvent (call.Session.CharacterID, OnItemChange.BuildLocationChange (item, oldFlag, oldLocationID));

        return null;
    }

    protected override long MachoResolveObject (CallInformation call, ServiceBindParams parameters)
    {
        throw new NotImplementedException ();
    }

    protected override BoundService CreateBoundInstance (CallInformation call, ServiceBindParams bindParams)
    {
        throw new NotImplementedException ();
    }
}