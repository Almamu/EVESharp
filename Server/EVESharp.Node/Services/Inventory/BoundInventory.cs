using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.Database.Corporations;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Old;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Dogma;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Exceptions.inventory;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types;
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
    private          IDogmaItems         DogmaItems         { get; }
    
    public BoundInventory
    (
        ItemDB              itemDB,             EffectsManager      effectsManager,     ItemInventory        item, Flags flag, IItems items,
        INotificationSender notificationSender, IDogmaNotifications dogmaNotifications, IBoundServiceManager manager, Session session,
        IDogmaItems dogmaItems
    ) : base (manager, session, item.ID)
    {
        EffectsManager     = effectsManager;
        this.mInventory    = item;
        this.mFlag         = flag;
        ItemDB             = itemDB;
        Items              = items;
        Notifications      = notificationSender;
        DogmaNotifications = dogmaNotifications;
        DogmaItems         = dogmaItems;
    }

    public BoundInventory
    (
        ItemDB              itemDB,             EffectsManager      effectsManager,     ItemInventory        item, IItems items,
        INotificationSender notificationSender, IDogmaNotifications dogmaNotifications, IBoundServiceManager manager, Session session,
        IDogmaItems dogmaItems
    ) : base (manager, session, item.ID)
    {
        EffectsManager     = effectsManager;
        this.mInventory    = item;
        this.mFlag         = Flags.None;
        ItemDB             = itemDB;
        Items              = items;
        Notifications      = notificationSender;
        DogmaNotifications = dogmaNotifications;
        DogmaItems         = dogmaItems;
    }

    public PyDataType List (ServiceCall call)
    {
        CRowset result = new CRowset (ItemEntity.EntityItemDescriptor);

        foreach ((int _, ItemEntity item) in this.mInventory.Items)
            if (this.mFlag == Flags.None || item.Flag == this.mFlag)
                result.Add (item.GetEntityRow ());

        return result;
    }

    public PyDataType ListStations (ServiceCall call, PyInteger blueprintsOnly, PyInteger forCorp)
    {
        int callerCharacterID = call.Session.CharacterID;

        return ItemDB.ListStations (forCorp == 1 ? call.Session.CorporationID : callerCharacterID, blueprintsOnly);
    }

    public PyDataType ListStationItems (ServiceCall call, PyInteger stationID)
    {
        return ItemDB.ListStationItems (stationID, call.Session.CharacterID);
    }

    public PyDataType ListStationBlueprintItems (ServiceCall call, PyInteger locationID, PyInteger _, PyInteger isCorp)
    {
        if (isCorp == 1)
            return ItemDB.ListStationBlueprintItems (locationID, call.Session.CorporationID);

        return ItemDB.ListStationBlueprintItems (locationID, call.Session.CharacterID);
    }

    public PyDataType GetItem (ServiceCall call)
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
        if (flag == Flags.Cargo)
        {
            // check destination cargo
            double currentVolume =
                this.mInventory.Items.Sum (x => x.Value.Flag != flag ? 0.0 : x.Value.Quantity * x.Value.Attributes [AttributeTypes.volume]);

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
            DogmaItems.FitInto (
                item, this.mInventory.ID, this.mInventory.OwnerID, newFlag, session
            );
        }
        else
        {
            DogmaItems.SplitStack (item, quantity == 0 ? item.Quantity : quantity, this.mInventory.ID, this.mInventory.OwnerID, newFlag);
        }
    }

    public PyDataType Add (ServiceCall call, PyInteger itemID)
    {
        if (itemID == call.Session.ShipID)
            throw new CantMoveActiveShip ();

        ItemEntity item = this.Items.GetItem (itemID);

        this.PreMoveItemCheck (item, this.mFlag, item.Quantity, call.Session);
        this.MoveItemHere (item, this.mFlag, call.Session);

        return null;
    }

    public PyDataType Add (ServiceCall call, PyInteger itemID, PyInteger quantity)
    {
        if (itemID == call.Session.ShipID)
            throw new CantMoveActiveShip ();

        ItemEntity item = this.Items.GetItem (itemID);

        this.PreMoveItemCheck (item, this.mFlag, item.Quantity, call.Session);
        this.MoveItemHere (item, this.mFlag, call.Session, quantity);

        return null;
    }

    public PyDataType Add (ServiceCall call, PyInteger itemID, PyInteger quantity, PyInteger flag)
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

    public PyDataType MultiAdd (ServiceCall call, PyList adds, PyInteger quantity, PyInteger flag)
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

    public PyDataType MultiAdd (ServiceCall call, PyList adds)
    {
        return this.MultiAdd (call, adds, null, (int) this.mFlag);
    }

    public PyDataType MultiMerge (ServiceCall call, PyList merges)
    {
        foreach (PyTuple merge in merges.GetEnumerable <PyTuple> ())
        {
            if (merge [0] is PyInteger == false || merge [1] is PyInteger == false || merge [2] is PyInteger == false)
                continue;

            PyInteger fromItemID = merge [0] as PyInteger;
            PyInteger toItemID   = merge [1] as PyInteger;
            PyInteger quantity   = merge [2] as PyInteger;
            
            DogmaItems.Merge (
                Items.LoadItem (toItemID),
                Items.LoadItem (fromItemID),
                quantity
            );
        }

        return null;
    }

    public PyDataType StackAll (ServiceCall call, PyString password)
    {
        throw new NotImplementedException ("Stacking on passworded containers is not supported yet!");
    }

    private void StackAll (ServiceCall call, Flags locationFlag)
    {
        // TODO: ADD CONSTRAINTS CHECKS FOR THE LOCATIONFLAG
        foreach ((int firstItemID, ItemEntity firstItem) in this.mInventory.Items)
        {
            // singleton items are not even checked
            if (firstItem.Singleton || firstItem.Flag != locationFlag)
                continue;

            foreach ((int secondItemID, ItemEntity secondItem) in this.mInventory.Items)
            {
                // ignore the same itemID as they cannot really be merged
                if (firstItemID == secondItemID || secondItem.Flag != locationFlag)
                    continue;
                if (firstItem.Type.ID != secondItem.Type.ID)
                    continue;

                DogmaItems.Merge (firstItem, secondItem);
                
                // finally break this loop as the merge was already done
                break;
            }
        }
    }

    public PyDataType StackAll (ServiceCall call, PyInteger locationFlag)
    {
        if (this.mFlag != Flags.None)
            return null;

        this.StackAll (call, (Flags) (int) locationFlag);

        return null;
    }

    public PyDataType StackAll (ServiceCall call)
    {
        if (this.mFlag == Flags.None)
            return null;

        this.StackAll (call, this.mFlag);

        return null;
    }

    public static PySubStruct BindInventory
    (
        ItemDB              itemDB,             EffectsManager      effectsManager,     ItemInventory        item, Flags flag, IItems items,
        INotificationSender notificationSender, IDogmaNotifications dogmaNotifications, IBoundServiceManager boundServiceManager, Session session,
        invbroker creator, IDogmaItems dogmaItems
    )
    {
        // create an instance of the inventory service and bind it to the item data
        // bind the service
        BoundInventory instance = new BoundInventory (
            itemDB, effectsManager, item, flag, items, notificationSender, dogmaNotifications,
            boundServiceManager, session, dogmaItems
        );

        int boundID = boundServiceManager.BindService (instance);
        // build the bound service string
        string boundServiceStr = boundServiceManager.BuildBoundServiceString (boundID);

        instance.BoundServiceInformation = new PyTuple (2)
        {
            [0] = boundServiceStr,
            [1] = Guid.NewGuid ().ToString () // ReferenceID, this should be unique
        };
        
        if (creator.Parent.BoundInventories.TryGetValue (item.ID, out List <BoundInventory> inventories) == false)
            inventories = creator.Parent.BoundInventories [item.ID] = new List <BoundInventory> ();

        inventories.Add (instance);

        // after the service is bound the call can be run (if required)
        return new PySubStruct (new PySubStream (instance.BoundServiceInformation));
    }

    public PyDataType BreakPlasticWrap (ServiceCall call, PyInteger crateID)
    {
        // TODO: ensure this item is a plastic wrap and mark the contract as void
        return null;
    }

    public PyDataType DestroyFitting (ServiceCall call, PyInteger itemID)
    {
        ItemEntity item = this.mInventory.Items [itemID];

        if (item.IsInRigSlot () == false)
            throw new CannotDestroyFittedItem ();

        if (item is ShipModule module)
            // disable passive effects
            EffectsManager.GetForItem (module, call.Session).StopApplyingPassiveEffects (call.Session);

        DogmaItems.DestroyItem (item);
        
        return null;
    }

    protected override long MachoResolveObject (ServiceCall call, ServiceBindParams parameters)
    {
        throw new NotImplementedException ();
    }

    protected override BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams)
    {
        throw new NotImplementedException ();
    }
}