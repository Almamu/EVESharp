using System;
using System.Collections.Generic;
using System.IO;
using EVESharp.Database;
using EVESharp.Database.Corporations;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Groups;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Data.Messages;
using EVESharp.EVE.Dogma;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Exceptions.inventory;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Dogma;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Container = EVESharp.Database.Inventory.Container;
using ItemDB = EVESharp.Database.Old.ItemDB;

namespace EVESharp.Node.Services.Inventory;

[MustBeCharacter]
public class invbroker : ClientBoundService
{
    private readonly int         mObjectID;
    public override  AccessLevel AccessLevel => AccessLevel.None;

    /// <summary>
    /// The list of bound inventories currently handled by this broker
    ///
    /// Key is inventoryID (itemID) and values are all the BoundInventories for that itemID
    /// </summary>
    public Dictionary <int, List <BoundInventory>> BoundInventories = new Dictionary <int, List <BoundInventory>> ();

    private IItems              Items              { get; }
    private ItemDB              ItemDB             { get; }
    private ISolarSystems       SolarSystems       { get; }
    private INotificationSender Notifications      { get; }
    private IDogmaNotifications DogmaNotifications { get; }
    private EffectsManager      EffectsManager     { get; }
    private IDatabase           Database           { get; }
    public  invbroker           Parent             { get; }
    private IDogmaItems         DogmaItems         { get; }

    public invbroker
    (
        ItemDB              itemDB,             EffectsManager       effectsManager, IItems items, INotificationSender notificationSender,
        IDogmaNotifications dogmaNotifications, IBoundServiceManager manager,        IDatabase database, ISolarSystems       solarSystems,
        IDogmaItems dogmaItems
    ) : base (manager)
    {
        EffectsManager     = effectsManager;
        Items              = items;
        ItemDB             = itemDB;
        Notifications      = notificationSender;
        DogmaNotifications = dogmaNotifications;
        Database           = database;
        SolarSystems       = solarSystems;
        DogmaItems         = dogmaItems;
    }

    private invbroker
    (
        ItemDB              itemDB,             EffectsManager       effectsManager, IItems items, INotificationSender notificationSender,
        IDogmaNotifications dogmaNotifications, IBoundServiceManager manager,        int    objectID, Session             session, ISolarSystems solarSystems,
        invbroker parent, IDogmaItems dogmaItems
    ) : base (manager, session, objectID)
    {
        EffectsManager     = effectsManager;
        Items              = items;
        ItemDB             = itemDB;
        this.mObjectID     = objectID;
        Notifications      = notificationSender;
        DogmaNotifications = dogmaNotifications;
        SolarSystems       = solarSystems;
        this.Parent        = parent;
        DogmaItems         = dogmaItems;
    }

    private PySubStruct BindInventory (ItemInventory inventory, Session session, Flags flag)
    {
        // create an instance of the inventory service and bind it to the item data
        return BoundInventory.BindInventory (
            ItemDB, EffectsManager, inventory, flag, this.Items, Notifications, this.DogmaNotifications,
            BoundServiceManager, session, this, DogmaItems
        );
    }

    public PySubStruct GetInventoryFromId (ServiceCall call, PyInteger itemID, PyInteger one)
    {
        ItemEntity item = Items.LoadItem (itemID);
        ItemInventory inventory = null;

        if (item is Station)
        {
            inventory = DogmaItems.LoadInventory (itemID, call.Session.CharacterID);
        }
        else if (item is OfficeFolder officeFolder)
        {
            // ensure the player has permissions to access this
            if (officeFolder.OwnerID != call.Session.CorporationID)
                throw new CrpAccessDenied ("Cannot access this inventory");
            
            inventory = officeFolder;
        }
        else if (item is Character character)
        {
            if (character.ID != call.Session.CharacterID)
                throw new CrpAccessDenied ("Cannot access this inventory");
            
            inventory = character;
        }
        else if (item is Ship ship)
        {
            if (ship.OwnerID != call.Session.CharacterID)
                throw new CrpAccessDenied ("Cannot access this inventory");

            inventory = ship;
        }
        else
        {
            throw new InvalidDataException ("Unknown item type on GetInventoryFromId");
        }

        return BindInventory (inventory, call.Session, Flags.None);
    }

    public PySubStruct GetInventory (ServiceCall call, PyInteger containerID, PyInteger origOwnerID)
    {
        int ownerID = call.Session.CharacterID;

        Flags flag;

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

        // load the inventory
        ItemInventory inventory = DogmaItems.LoadInventory (this.mObjectID, ownerID);
        
        return BindInventory (inventory, call.Session, flag);
    }

    public PyDataType TrashItems (ServiceCall call, PyList itemIDs, PyInteger stationID)
    {
        foreach (PyInteger itemID in itemIDs.GetEnumerable <PyInteger> ())
        {
            // do not trash the active ship
            if (itemID == call.Session.ShipID)
                throw new CantMoveActiveShip ();

            DogmaItems.DestroyItem (Items.GetItem (itemID));
        }

        return null;
    }

    public PyDataType SetLabel (ServiceCall call, PyInteger itemID, PyString newLabel)
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

        return null;
    }

    public PyDataType AssembleCargoContainer (ServiceCall call, PyInteger containerID, PyDataType ignored, PyDecimal ignored2)
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

    public PyDataType DeliverToCorpHangar (ServiceCall call, PyInteger stationID, PyList itemIDs, PyDataType quantity, PyInteger ownerID, PyInteger deliverToFlag)
    {
        // TODO: DETERMINE IF THIS FUNCTION HAS TO BE IMPLEMENTED
        // LIVE CCP SERVER DOES NOT SUPPORT IT, EVEN THO THE MENU OPTION IS SHOWN TO THE USER
        return null;
    }

    public PyDataType DeliverToCorpMember (ServiceCall call, PyInteger memberID, PyInteger stationID, PyList itemIDs, PyDataType quantity, PyInteger ownerID)
    {
        return null;
    }

    protected override long MachoResolveObject (ServiceCall call, ServiceBindParams parameters)
    {
        return parameters.ExtraValue switch
        {
            (int) GroupID.SolarSystem => Database.CluResolveAddress ("solarsystem", parameters.ObjectID),
            (int) GroupID.Station     => Database.CluResolveAddress ("station",     parameters.ObjectID),
            _                         => throw new CustomError ("Unknown item's groupID")
        };
    }

    protected override BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams)
    {
        if (this.MachoResolveObject (call, bindParams) != call.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        return new invbroker (
            ItemDB, EffectsManager, this.Items, Notifications, this.DogmaNotifications, BoundServiceManager, bindParams.ObjectID,
            call.Session, this.SolarSystems, this, DogmaItems
        );
    }
}