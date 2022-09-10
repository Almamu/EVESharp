using EVESharp.Database;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Data.Messages;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Exceptions.inventory;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Dogma;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Container = EVESharp.EVE.Data.Inventory.Container;
using ItemDB = EVESharp.Database.Old.ItemDB;

namespace EVESharp.Node.Services.Inventory;

[MustBeCharacter]
public class invbroker : ClientBoundService
{
    private readonly int         mObjectID;
    public override  AccessLevel AccessLevel => AccessLevel.None;

    private IItems              Items              { get; }
    private ItemDB              ItemDB             { get; }
    private ISolarSystems       SolarSystems       { get; }
    private INotificationSender Notifications      { get; }
    private IDogmaNotifications DogmaNotifications { get; }
    private EffectsManager      EffectsManager     { get; }
    private IDatabaseConnection Database           { get; }

    public invbroker
    (
        ItemDB              itemDB,             EffectsManager      effectsManager, IItems              items,    INotificationSender notificationSender,
        IDogmaNotifications dogmaNotifications, BoundServiceManager manager,        IDatabaseConnection database, ISolarSystems       solarSystems
    ) : base (manager)
    {
        EffectsManager     = effectsManager;
        Items              = items;
        ItemDB             = itemDB;
        Notifications      = notificationSender;
        DogmaNotifications = dogmaNotifications;
        Database           = database;
        SolarSystems       = solarSystems;
    }

    private invbroker
    (
        ItemDB              itemDB,             EffectsManager      effectsManager, IItems items,    INotificationSender notificationSender,
        IDogmaNotifications dogmaNotifications, BoundServiceManager manager,        int    objectID, Session             session, ISolarSystems solarSystems
    ) : base (manager, session, objectID)
    {
        EffectsManager     = effectsManager;
        Items              = items;
        ItemDB             = itemDB;
        this.mObjectID     = objectID;
        Notifications      = notificationSender;
        DogmaNotifications = dogmaNotifications;
        SolarSystems       = solarSystems;
    }

    private ItemInventory CheckInventoryBeforeLoading (ItemEntity inventoryItem)
    {
        // also make sure it's a container
        if (inventoryItem is ItemInventory == false)
            throw new ItemNotContainer (inventoryItem.ID);

        // extra check, ensure it's a singleton if not a station
        if (inventoryItem.Type.Group.ID != (int) GroupID.Station && inventoryItem.Singleton == false)
            throw new AssembleCCFirst ();

        return (ItemInventory) inventoryItem;
    }

    private PySubStruct BindInventory (ItemInventory inventoryItem, int ownerID, Session session, Flags flag)
    {
        ItemInventory inventory = inventoryItem;

        // create a meta inventory only if required
        if (inventoryItem is not Ship && inventoryItem is not Character)
            inventory = this.Items.MetaInventories.RegisterMetaInventoryForOwnerID (inventoryItem, ownerID, flag);

        // create an instance of the inventory service and bind it to the item data
        return BoundInventory.BindInventory (
            ItemDB, EffectsManager, inventory, flag, this.Items, Notifications, this.DogmaNotifications,
            BoundServiceManager, session
        );
    }

    public PySubStruct GetInventoryFromId (CallInformation call, PyInteger itemID, PyInteger one)
    {
        int        ownerID       = call.Session.CharacterID;
        ItemEntity inventoryItem = this.Items.LoadItem (itemID);

        if (inventoryItem is not Station)
            ownerID = inventoryItem.OwnerID;

        return this.BindInventory (
            this.CheckInventoryBeforeLoading (inventoryItem),
            ownerID,
            call.Session, Flags.None
        );
    }

    public PySubStruct GetInventory (CallInformation call, PyInteger containerID, PyInteger origOwnerID)
    {
        int ownerID = call.Session.CharacterID;

        Flags flag = Flags.None;

        switch ((int) containerID)
        {
            case (int) Container.Wallet:
                flag = Flags.Wallet;
                break;

            case (int) Container.Hangar:
                flag = Flags.Hangar;

                if (origOwnerID is not null)
                    ownerID = origOwnerID;

                if (ownerID != call.Session.CharacterID && ownerID != call.Session.CorporationID)
                    throw new CrpAccessDenied (MLS.UI_CORP_ACCESSDENIED13);

                if (ownerID == call.Session.CorporationID && CorporationRole.SecurityOfficer.Is (call.Session.CorporationRole) == false)
                    throw new CrpAccessDenied (MLS.UI_CORP_ACCESSDENIED13);
                break;

            case (int) Container.Character:
                flag = Flags.Skill;
                break;

            case (int) Container.Global:
                flag = Flags.None;
                break;

            case (int) Container.CorpMarket:
                flag    = Flags.CorpMarket;
                ownerID = call.Session.CorporationID;

                // check permissions
                if (CorporationRole.Accountant.Is (call.Session.CorporationRole) == false &&
                    CorporationRole.JuniorAccountant.Is (call.Session.CorporationRole) == false &&
                    CorporationRole.Trader.Is (call.Session.CorporationRole) == false)
                    throw new CrpAccessDenied (MLS.UI_CORP_ACCESSDENIED14);
                break;

            default: throw new CustomError ($"Trying to open container ID ({containerID.Value}) is not supported");
        }

        // these inventories are usually meta

        // get the inventory item first
        ItemInventory inventoryItem = this.Items.LoadItem <ItemInventory> (this.mObjectID);

        // create a metainventory for it
        ItemInventory metaInventory = this.Items.MetaInventories.RegisterMetaInventoryForOwnerID (inventoryItem, ownerID, flag);

        return this.BindInventory (
            this.CheckInventoryBeforeLoading (metaInventory),
            ownerID,
            call.Session, flag
        );
    }

    public PyDataType TrashItems (CallInformation call, PyList itemIDs, PyInteger stationID)
    {
        int callerCharacterID = call.Session.CharacterID;

        foreach (PyInteger itemID in itemIDs.GetEnumerable <PyInteger> ())
        {
            // do not trash the active ship
            if (itemID == call.Session.ShipID)
                throw new CantMoveActiveShip ();

            ItemEntity item = this.Items.GetItem (itemID);
            // store it's location id
            int   oldLocation = item.LocationID;
            Flags oldFlag     = item.Flag;
            // remove the item off the ItemManager
            this.Items.DestroyItem (item);
            // notify the client of the change
            this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildLocationChange (item, oldFlag, oldLocation));
            // TODO: CHECK IF THE ITEM HAS ANY META INVENTORY AND/OR BOUND SERVICE
            // TODO: AND FREE THOSE TOO SO THE ITEMS CAN BE REMOVED OFF THE DATABASE
        }

        return null;
    }

    public PyDataType SetLabel (CallInformation call, PyInteger itemID, PyString newLabel)
    {
        ItemEntity item = this.Items.GetItem (itemID);

        // ensure the itemID is owned by the client's character
        if (item.OwnerID != call.Session.CharacterID)
            throw new TheItemIsNotYoursToTake (itemID);

        item.Name = newLabel;

        // ensure the item is saved into the database first
        item.Persist ();

        // notify the owner of the item
        Notifications.NotifyOwner (item.OwnerID, OnCfgDataChanged.BuildItemLabelChange (item));

        // TODO: CHECK IF ITEM BELONGS TO CORP AND NOTIFY CHARACTERS IN THIS NODE?
        return null;
    }

    public PyDataType AssembleCargoContainer
    (
        CallInformation call, PyInteger containerID, PyDataType ignored, PyDecimal ignored2
    )
    {
        ItemEntity item = this.Items.GetItem (containerID);

        if (item.OwnerID != call.Session.CharacterID)
            throw new TheItemIsNotYoursToTake (containerID);

        // ensure the item is a cargo container
        switch (item.Type.Group.ID)
        {
            case (int) GroupID.CargoContainer:
            case (int) GroupID.SecureCargoContainer:
            case (int) GroupID.AuditLogSecureContainer:
            case (int) GroupID.FreightContainer:
            case (int) GroupID.Tool:
            case (int) GroupID.MobileWarpDisruptor:
                break;
            default: throw new ItemNotContainer (containerID);
        }

        bool oldSingleton = item.Singleton;

        // update singleton
        item.Singleton = true;
        item.Persist ();

        // notify the client
        this.DogmaNotifications.QueueMultiEvent (item.OwnerID, OnItemChange.BuildSingletonChange (item, oldSingleton));

        return null;
    }

    public PyDataType DeliverToCorpHangar
    (
        CallInformation call, PyInteger stationID, PyList itemIDs, PyDataType quantity, PyInteger ownerID, PyInteger deliverToFlag
    )
    {
        // TODO: DETERMINE IF THIS FUNCTION HAS TO BE IMPLEMENTED
        // LIVE CCP SERVER DOES NOT SUPPORT IT, EVEN THO THE MENU OPTION IS SHOWN TO THE USER
        return null;
    }

    public PyDataType DeliverToCorpMember
    (
        CallInformation call, PyInteger memberID, PyInteger stationID, PyList itemIDs, PyDataType quantity, PyInteger ownerID
    )
    {
        return null;
    }

    protected override long MachoResolveObject (CallInformation call, ServiceBindParams parameters)
    {
        return parameters.ExtraValue switch
        {
            (int) GroupID.SolarSystem => Database.CluResolveAddress ("solarsystem", parameters.ObjectID),
            (int) GroupID.Station     => Database.CluResolveAddress ("station",     parameters.ObjectID),
            _                         => throw new CustomError ("Unknown item's groupID")
        };
    }

    protected override BoundService CreateBoundInstance (CallInformation call, ServiceBindParams bindParams)
    {
        if (this.MachoResolveObject (call, bindParams) != call.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        return new invbroker (
            ItemDB, EffectsManager, this.Items, Notifications, this.DogmaNotifications, BoundServiceManager, bindParams.ObjectID,
            call.Session, this.SolarSystems
        );
    }
}