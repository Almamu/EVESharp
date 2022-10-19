using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Market;
using EVESharp.Database.Old;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.repairSvc;
using EVESharp.EVE.Market;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types;
using EVESharp.Node.Server.Shared.Exceptions;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Service = EVESharp.Database.Inventory.Stations.Service;

namespace EVESharp.Node.Services.Stations;

[MustBeCharacter]
public class repairSvc : ClientBoundService
{
    private const    double              BASEPRICE_MULTIPLIER_MODULE = 0.0125;
    private const    double              BASEPRICE_MULTIPLIER_SHIP   = 0.000088;
    private readonly ItemInventory       mInventory;
    public override  AccessLevel         AccessLevel        => AccessLevel.None;
    private          IItems              Items              { get; }
    private          ISolarSystems       SolarSystems       { get; }
    private          ITypes              Types              => this.Items.Types;
    private          MarketDB            MarketDB           { get; }
    private          RepairDB            RepairDB           { get; }
    private          InsuranceDB         InsuranceDB        { get; }
    private          INotificationSender Notifications      { get; }
    private          IWallets            Wallets            { get; }
    private          IDogmaNotifications DogmaNotifications { get; }
    private          IDatabase Database           { get; }

    public repairSvc
    (
        RepairDB      repairDb,     MarketDB             marketDb, InsuranceDB insuranceDb, INotificationSender notificationSender,
        IItems        items,        IBoundServiceManager manager,  IWallets    wallets,     IDogmaNotifications dogmaNotifications,
        ISolarSystems solarSystems, IDatabase  database
    ) : base (manager)
    {
        Items              = items;
        MarketDB           = marketDb;
        RepairDB           = repairDb;
        InsuranceDB        = insuranceDb;
        Notifications      = notificationSender;
        this.Wallets       = wallets;
        DogmaNotifications = dogmaNotifications;
        Database           = database;
        SolarSystems       = solarSystems;
    }

    protected repairSvc
    (
        RepairDB      repairDb,  MarketDB marketDb, InsuranceDB          insuranceDb, INotificationSender notificationSender,
        ItemInventory inventory, IItems   items,    IBoundServiceManager manager,     IWallets wallets, IDogmaNotifications dogmaNotifications, Session session
    ) : base (manager, session, inventory.ID)
    {
        this.mInventory         = inventory;
        this.Items              = items;
        MarketDB                = marketDb;
        RepairDB                = repairDb;
        InsuranceDB             = insuranceDb;
        Notifications           = notificationSender;
        this.Wallets            = wallets;
        this.DogmaNotifications = dogmaNotifications;
    }

    public PyDataType GetDamageReports (ServiceCall call, PyList itemIDs)
    {
        PyDictionary <PyInteger, PyDataType> response = new PyDictionary <PyInteger, PyDataType> ();

        foreach (PyInteger itemID in itemIDs.GetEnumerable <PyInteger> ())
        {
            // ensure the given item is in the list
            if (this.mInventory.Items.TryGetValue (itemID, out ItemEntity item) == false)
                continue;

            Rowset quote = new Rowset (
                new PyList <PyString> (6)
                {
                    [0] = "itemID",
                    [1] = "typeID",
                    [2] = "groupID",
                    [3] = "damage",
                    [4] = "maxHealth",
                    [5] = "costToRepairOneUnitOfDamage"
                }
            );

            if (item is Ship ship)
            {
                foreach ((int _, ItemEntity module) in ship.Items)
                {
                    if (module.IsInModuleSlot () == false && module.IsInRigSlot () == false)
                        continue;

                    quote.Rows.Add (
                        new PyList
                        {
                            module.ID,
                            module.Type.ID,
                            module.Type.Group.ID,
                            module.Attributes [AttributeTypes.damage],
                            module.Attributes [AttributeTypes.hp],
                            // modules should calculate this value differently, but for now this will suffice
                            module.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE
                        }
                    );
                }

                quote.Rows.Add (
                    new PyList
                    {
                        item.ID,
                        item.Type.ID,
                        item.Type.Group.ID,
                        item.Attributes [AttributeTypes.damage],
                        item.Attributes [AttributeTypes.hp],
                        item.Type.BasePrice * BASEPRICE_MULTIPLIER_SHIP
                    }
                );
            }
            else
            {
                quote.Rows.Add (
                    new PyList
                    {
                        item.ID,
                        item.Type.ID,
                        item.Type.Group.ID,
                        item.Attributes [AttributeTypes.damage],
                        item.Attributes [AttributeTypes.hp],
                        item.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE
                    }
                );
            }

            // the client used to send a lot of extra information on this call
            // but in reality that data is not used by the client at all
            // most likely remnants of older eve client versions
            response [itemID] = new Row (
                new PyList <PyString> (1) {[0] = "quote"},
                new PyList (1) {[0]            = quote}
            );
        }

        return response;
    }

    [MustBeInStation]
    public PyDataType RepairItems (ServiceCall call, PyList itemIDs, PyDecimal iskRepairValue)
    {
        // ensure the player has enough balance to do the fixing
        Station station = this.Items.GetStaticStation (call.Session.StationID);

        // take the wallet lock and ensure the character has enough balance
        using IWallet wallet = this.Wallets.AcquireWallet (call.Session.CharacterID, WalletKeys.MAIN);

        {
            wallet.EnsureEnoughBalance (iskRepairValue);
            // build a list of items to be fixed
            List <ItemEntity> items = new List <ItemEntity> ();

            double quantityLeft = iskRepairValue;

            foreach (PyInteger itemID in itemIDs.GetEnumerable <PyInteger> ())
            {
                // ensure the given item is in the list
                if (this.mInventory.Items.TryGetValue (itemID, out ItemEntity item) == false)
                    continue;

                // calculate how much to fix it
                if (item is Ship)
                    quantityLeft -= Math.Min (item.Attributes [AttributeTypes.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_SHIP), quantityLeft);
                else
                    quantityLeft -= Math.Min (item.Attributes [AttributeTypes.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE), quantityLeft);

                // add the item to the list
                items.Add (item);

                // if there's not enough money left then break the loop and fix whatever's possible 
                if (quantityLeft <= 0.0)
                    break;
            }

            quantityLeft = iskRepairValue;

            // go through all the items again and fix them
            foreach (ItemEntity item in items)
            {
                double repairPrice = 0.0f;

                if (item is Ship)
                    repairPrice = item.Attributes [AttributeTypes.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_SHIP);
                else
                    repairPrice = item.Attributes [AttributeTypes.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE);

                // full item can be repaired!
                if (repairPrice <= quantityLeft)
                {
                    item.Attributes [AttributeTypes.damage].Integer = 0;
                }
                else
                {
                    int repairUnits = 0;

                    // calculate how much can be repaired with the quantity left
                    if (item is Ship)
                    {
                        repairUnits = (int) (quantityLeft / (item.Type.BasePrice * BASEPRICE_MULTIPLIER_SHIP));
                        repairPrice = repairUnits * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_SHIP);
                    }
                    else
                    {
                        repairUnits = (int) (quantityLeft / (item.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE));
                        repairPrice = repairUnits * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE);
                    }

                    // only perform changes on the damage if there's units we can pay for repair
                    if (repairUnits > 0)
                        item.Attributes [AttributeTypes.damage] -= repairUnits;
                }

                quantityLeft -= repairPrice;
                // persist item changes
                item.Persist ();
            }

            wallet.CreateJournalRecord (MarketReference.RepairBill, station.OwnerID, null, -(iskRepairValue - quantityLeft));
        }

        return null;
    }

    public PyDataType UnasembleItems (ServiceCall call, PyDictionary validIDsByStationID, PyList skipChecks)
    {
        int                                characterID = call.Session.CharacterID;
        List <RepairDB.ItemRepackageEntry> entries     = new List <RepairDB.ItemRepackageEntry> ();

        bool ignoreContractVoiding       = false;
        bool ignoreRepackageWithUpgrades = false;

        foreach (PyString check in skipChecks.GetEnumerable <PyString> ())
        {
            if (check == "RepairUnassembleVoidsContract")
                ignoreContractVoiding = true;
            else if (check == "ConfirmRepackageSomethingWithUpgrades")
                ignoreRepackageWithUpgrades = true;
        }

        List <int> stationIDs = new List <int> ();
        
        // get the first station id and check if the station belongs to us
        foreach ((PyInteger currentStationID, PyList _) in validIDsByStationID.GetEnumerable <PyInteger, PyList> ())
        {
            stationIDs.Add (currentStationID);
        }

        if (stationIDs.Distinct ().Count () > 1)
            throw new CustomError ("Cannot repair items from more than one station at the same time");

        int stationID = stationIDs.First ();
        int nodeID    = (int) this.SolarSystems.GetNodeStationBelongsTo (stationID);

        if (nodeID != call.MachoNet.NodeID && nodeID != 0)
            throw new RedirectCallRequest (nodeID);

        foreach ((PyInteger _, PyList itemIDs) in validIDsByStationID.GetEnumerable <PyInteger, PyList> ())
        {
            foreach (PyInteger itemID in itemIDs.GetEnumerable <PyInteger> ())
            {
                RepairDB.ItemRepackageEntry entry = RepairDB.GetItemToRepackage (itemID, characterID, stationID);

                if (entry.HasContract && ignoreContractVoiding == false)
                    throw new RepairUnassembleVoidsContract (itemID);

                if (entry.HasUpgrades && ignoreRepackageWithUpgrades == false)
                    throw new ConfirmRepackageSomethingWithUpgrades ();

                if (entry.Damage != 0.0)
                    throw new CantRepackageDamagedItem ();

                entries.Add (entry);
            }
        }

        foreach (RepairDB.ItemRepackageEntry entry in entries)
        {
            if (entry.Singleton == false)
                continue;

            ItemEntity item = this.Items.LoadItem (entry.ItemID, out bool loadRequired);

            // the item is an inventory, take everything out!
            if (item is ItemInventory inventory)
            {
                foreach ((int _, ItemEntity itemInInventory) in inventory.Items)
                {
                    // if the item is in a rig slot, destroy it
                    if (itemInInventory.IsInRigSlot ())
                    {
                        Flags oldFlag = itemInInventory.Flag;
                        this.Items.DestroyItem (itemInInventory);
                        // notify the client about the change
                        this.DogmaNotifications.QueueMultiEvent (characterID, OnItemChange.BuildLocationChange (itemInInventory, oldFlag, entry.ItemID));
                    }
                    else
                    {
                        Flags oldFlag = itemInInventory.Flag;
                        // update item's location
                        itemInInventory.LocationID = entry.LocationID;
                        itemInInventory.Flag       = Flags.Hangar;

                        // notify the client about the change
                        this.DogmaNotifications.QueueMultiEvent (characterID, OnItemChange.BuildLocationChange (itemInInventory, oldFlag, entry.ItemID));
                        // save the item
                        itemInInventory.Persist ();
                    }
                }
            }

            // update the singleton flag too
            item.Singleton = false;
            this.DogmaNotifications.QueueMultiEvent (characterID, OnItemChange.BuildSingletonChange (item, true));

            // load was required, the item is not needed anymore
            if (loadRequired)
                this.Items.UnloadItem (item);

            // finally repackage the item
            RepairDB.RepackageItem (entry.ItemID, entry.LocationID);
            // remove any insurance contract for the ship
            InsuranceDB.UnInsureShip (entry.ItemID);
        }

        return null;
    }

    protected override long MachoResolveObject (ServiceCall call, ServiceBindParams parameters)
    {
        return Database.CluResolveAddress ("station", parameters.ObjectID);
    }

    protected override BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams)
    {
        if (this.MachoResolveObject (call, bindParams) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        Station station = this.Items.GetStaticStation (bindParams.ObjectID);

        if (station.HasService (Service.RepairFacilities) == false)
            throw new CustomError ("This station does not allow for repair facilities services");

        // ensure the player is in this station
        if (station.ID != call.Session.StationID)
            throw new CanOnlyDoInStations ();

        ItemInventory inventory =
            this.Items.MetaInventories.RegisterMetaInventoryForOwnerID (station, call.Session.CharacterID, Flags.Hangar);

        return new repairSvc (
            RepairDB, MarketDB, InsuranceDB, Notifications, inventory, this.Items, BoundServiceManager,
            this.Wallets, this.DogmaNotifications, call.Session
        );
    }
}