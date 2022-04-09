using EVESharp.EVE;
using EVESharp.EVE.Client.Exceptions.corpRegistry;
using EVESharp.EVE.Client.Exceptions.inventory;
using EVESharp.EVE.Client.Messages;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Corporation;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Database;
using EVESharp.Node.Dogma;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using Container = EVESharp.EVE.StaticData.Inventory.Container;

namespace EVESharp.Node.Services.Inventory;

public class invbroker : ClientBoundService
{
    private readonly int         mObjectID;
    public override  AccessLevel AccessLevel => AccessLevel.None;

    private ItemFactory         ItemFactory         { get; }
    private ItemDB              ItemDB              { get; }
    private NodeContainer       NodeContainer       { get; }
    private SystemManager       SystemManager       => ItemFactory.SystemManager;
    private NotificationManager NotificationManager { get; }
    private Node.Dogma.Dogma    Dogma               { get; }
    private EffectsManager      EffectsManager      { get; }

    public invbroker (
        ItemDB           itemDB, EffectsManager effectsManager, ItemFactory itemFactory, NodeContainer nodeContainer, NotificationManager notificationManager,
        Node.Dogma.Dogma dogma,  BoundServiceManager manager
    ) : base (manager)
    {
        EffectsManager      = effectsManager;
        ItemFactory         = itemFactory;
        ItemDB              = itemDB;
        NodeContainer       = nodeContainer;
        NotificationManager = notificationManager;
        Dogma               = dogma;
    }

    private invbroker (
        ItemDB           itemDB, EffectsManager effectsManager, ItemFactory itemFactory, NodeContainer nodeContainer, NotificationManager notificationManager,
        Node.Dogma.Dogma dogma,  BoundServiceManager manager, int objectID, Session session
    ) : base (manager, session, objectID)
    {
        EffectsManager      = effectsManager;
        ItemFactory         = itemFactory;
        ItemDB              = itemDB;
        this.mObjectID      = objectID;
        NodeContainer       = nodeContainer;
        NotificationManager = notificationManager;
        Dogma               = dogma;
    }

    private ItemInventory CheckInventoryBeforeLoading (ItemEntity inventoryItem)
    {
        // also make sure it's a container
        if (inventoryItem is ItemInventory == false)
            throw new ItemNotContainer (inventoryItem.ID);

        // extra check, ensure it's a singleton if not a station
        if (inventoryItem.Type.Group.ID != (int) Groups.Station && inventoryItem.Singleton == false)
            throw new AssembleCCFirst ();

        return (ItemInventory) inventoryItem;
    }

    private PySubStruct BindInventory (ItemInventory inventoryItem, int ownerID, Session session, Flags flag)
    {
        ItemInventory inventory = inventoryItem;

        // create a meta inventory only if required
        if (inventoryItem is not Ship && inventoryItem is not Character)
            inventory = ItemFactory.MetaInventoryManager.RegisterMetaInventoryForOwnerID (inventoryItem, ownerID, flag);

        // create an instance of the inventory service and bind it to the item data
        return BoundInventory.BindInventory (
            ItemDB, EffectsManager, inventory, flag, ItemFactory, NodeContainer, NotificationManager, Dogma,
            BoundServiceManager, session
        );
    }

    public PySubStruct GetInventoryFromId (PyInteger itemID, PyInteger one, CallInformation call)
    {
        int        ownerID       = call.Session.EnsureCharacterIsSelected ();
        ItemEntity inventoryItem = ItemFactory.LoadItem (itemID);

        if (inventoryItem is not Station)
            ownerID = inventoryItem.OwnerID;

        return this.BindInventory (
            this.CheckInventoryBeforeLoading (inventoryItem),
            ownerID,
            call.Session, Flags.None
        );
    }

    public PySubStruct GetInventory (PyInteger containerID, PyInteger origOwnerID, CallInformation call)
    {
        int ownerID = call.Session.EnsureCharacterIsSelected ();

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

            default:
                throw new CustomError ($"Trying to open container ID ({containerID.Value}) is not supported");
        }

        // these inventories are usually meta

        // get the inventory item first
        ItemInventory inventoryItem = ItemFactory.LoadItem <ItemInventory> (this.mObjectID);

        // create a metainventory for it
        ItemInventory metaInventory = ItemFactory.MetaInventoryManager.RegisterMetaInventoryForOwnerID (inventoryItem, ownerID, flag);

        return this.BindInventory (
            this.CheckInventoryBeforeLoading (metaInventory),
            ownerID,
            call.Session, flag
        );
    }

    public PyDataType TrashItems (PyList itemIDs, PyInteger stationID, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        foreach (PyInteger itemID in itemIDs.GetEnumerable <PyInteger> ())
        {
            // do not trash the active ship
            if (itemID == call.Session.ShipID)
                throw new CantMoveActiveShip ();

            ItemEntity item = ItemFactory.GetItem (itemID);
            // store it's location id
            int   oldLocation = item.LocationID;
            Flags oldFlag     = item.Flag;
            // remove the item off the ItemManager
            ItemFactory.DestroyItem (item);
            // notify the client of the change
            Dogma.QueueMultiEvent (callerCharacterID, OnItemChange.BuildLocationChange (item, oldFlag, oldLocation));
            // TODO: CHECK IF THE ITEM HAS ANY META INVENTORY AND/OR BOUND SERVICE
            // TODO: AND FREE THOSE TOO SO THE ITEMS CAN BE REMOVED OFF THE DATABASE
        }

        return null;
    }

    public PyDataType SetLabel (PyInteger itemID, PyString newLabel, CallInformation call)
    {
        ItemEntity item = ItemFactory.GetItem (itemID);

        // ensure the itemID is owned by the client's character
        if (item.OwnerID != call.Session.EnsureCharacterIsSelected ())
            throw new TheItemIsNotYoursToTake (itemID);

        item.Name = newLabel;

        // ensure the item is saved into the database first
        item.Persist ();

        // notify the owner of the item
        NotificationManager.NotifyOwner (item.OwnerID, OnCfgDataChanged.BuildItemLabelChange (item));

        // TODO: CHECK IF ITEM BELONGS TO CORP AND NOTIFY CHARACTERS IN THIS NODE?
        return null;
    }

    public PyDataType AssembleCargoContainer (
        PyInteger       containerID, PyDataType ignored, PyDecimal ignored2,
        CallInformation call
    )
    {
        ItemEntity item = ItemFactory.GetItem (containerID);

        if (item.OwnerID != call.Session.EnsureCharacterIsSelected ())
            throw new TheItemIsNotYoursToTake (containerID);

        // ensure the item is a cargo container
        switch (item.Type.Group.ID)
        {
            case (int) Groups.CargoContainer:
            case (int) Groups.SecureCargoContainer:
            case (int) Groups.AuditLogSecureContainer:
            case (int) Groups.FreightContainer:
            case (int) Groups.Tool:
            case (int) Groups.MobileWarpDisruptor:
                break;
            default:
                throw new ItemNotContainer (containerID);
        }

        bool oldSingleton = item.Singleton;

        // update singleton
        item.Singleton = true;
        item.Persist ();

        // notify the client
        Dogma.QueueMultiEvent (item.OwnerID, OnItemChange.BuildSingletonChange (item, oldSingleton));

        return null;
    }

    public PyDataType DeliverToCorpHangar (
        PyInteger stationID, PyList itemIDs, PyDataType quantity, PyInteger ownerID, PyInteger deliverToFlag, CallInformation call
    )
    {
        // TODO: DETERMINE IF THIS FUNCTION HAS TO BE IMPLEMENTED
        // LIVE CCP SERVER DOES NOT SUPPORT IT, EVEN THO THE MENU OPTION IS SHOWN TO THE USER
        return null;
    }

    public PyDataType DeliverToCorpMember (
        PyInteger memberID, PyInteger stationID, PyList itemIDs, PyDataType quantity, PyInteger ownerID, CallInformation call
    )
    {
        return null;
    }

    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        int solarSystemID = 0;

        if (parameters.ExtraValue == (int) Groups.SolarSystem)
            solarSystemID = ItemFactory.GetStaticSolarSystem (parameters.ObjectID).ID;
        else if (parameters.ExtraValue == (int) Groups.Station)
            solarSystemID = ItemFactory.GetStaticStation (parameters.ObjectID).SolarSystemID;
        else
            throw new CustomError ("Unknown item's groupID");

        return SystemManager.LoadSolarSystemOnCluster (solarSystemID);
    }

    protected override BoundService CreateBoundInstance (ServiceBindParams bindParams, CallInformation call)
    {
        if (this.MachoResolveObject (bindParams, call) != call.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        return new invbroker (
            ItemDB, EffectsManager, ItemFactory, NodeContainer, NotificationManager, Dogma, BoundServiceManager, bindParams.ObjectID,
            call.Session
        );
    }
}