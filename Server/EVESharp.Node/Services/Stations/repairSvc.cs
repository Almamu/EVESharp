using System;
using System.Collections.Generic;
using EVESharp.EVE.Client.Exceptions;
using EVESharp.EVE.Client.Exceptions.repairSvc;
using EVESharp.EVE.Market;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.EVE.Wallet;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Service = EVESharp.EVE.StaticData.Inventory.Station.Service;

namespace EVESharp.Node.Services.Stations;

public class repairSvc : ClientBoundService
{
    private const    double                      BASEPRICE_MULTIPLIER_MODULE = 0.0125;
    private const    double                      BASEPRICE_MULTIPLIER_SHIP   = 0.000088;
    private readonly ItemInventory               mInventory;
    public override  AccessLevel                 AccessLevel         => AccessLevel.None;
    private          ItemFactory                 ItemFactory         { get; }
    private          SystemManager               SystemManager       => ItemFactory.SystemManager;
    private          TypeManager                 TypeManager         => ItemFactory.TypeManager;
    private          MarketDB                    MarketDB            { get; }
    private          RepairDB                    RepairDB            { get; }
    private          InsuranceDB                 InsuranceDB         { get; }
    private          Notifications.Notifications Notifications { get; }
    private          WalletManager               WalletManager       { get; }
    private          Node.Dogma.Dogma            Dogma               { get; }

    public repairSvc (
        RepairDB    repairDb,    MarketDB            marketDb, InsuranceDB   insuranceDb, Notifications.Notifications notifications,
        ItemFactory itemFactory, BoundServiceManager manager,  WalletManager walletManager, Node.Dogma.Dogma dogma
    ) : base (manager)
    {
        ItemFactory   = itemFactory;
        MarketDB      = marketDb;
        RepairDB      = repairDb;
        InsuranceDB   = insuranceDb;
        Notifications = notifications;
        WalletManager = walletManager;
        Dogma         = dogma;
    }

    protected repairSvc (
        RepairDB      repairDb,  MarketDB    marketDb,    InsuranceDB         insuranceDb, Notifications.Notifications notifications,
        ItemInventory inventory, ItemFactory itemFactory, BoundServiceManager manager,     WalletManager walletManager, Node.Dogma.Dogma dogma, Session session
    ) : base (manager, session, inventory.ID)
    {
        this.mInventory     = inventory;
        ItemFactory         = itemFactory;
        MarketDB            = marketDb;
        RepairDB            = repairDb;
        InsuranceDB         = insuranceDb;
        Notifications = notifications;
        WalletManager       = walletManager;
        Dogma               = dogma;
    }

    public PyDataType GetDamageReports (PyList itemIDs, CallInformation call)
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

    public PyDataType RepairItems (PyList itemIDs, PyDecimal iskRepairValue, CallInformation call)
    {
        // ensure the player has enough balance to do the fixing
        Station station = ItemFactory.GetStaticStation (call.Session.EnsureCharacterIsInStation ());

        // take the wallet lock and ensure the character has enough balance
        using Wallet wallet = WalletManager.AcquireWallet (call.Session.EnsureCharacterIsSelected (), Keys.MAIN);
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

    public PyDataType UnasembleItems (PyDictionary validIDsByStationID, PyList skipChecks, CallInformation call)
    {
        int                                characterID = call.Session.EnsureCharacterIsSelected ();
        List <RepairDB.ItemRepackageEntry> entries     = new List <RepairDB.ItemRepackageEntry> ();

        bool ignoreContractVoiding       = false;
        bool ignoreRepackageWithUpgrades = false;

        foreach (PyString check in skipChecks.GetEnumerable <PyString> ())
        {
            if (check == "RepairUnassembleVoidsContract")
                ignoreContractVoiding = true;
            if (check == "ConfirmRepackageSomethingWithUpgrades")
                ignoreRepackageWithUpgrades = true;
        }

        foreach ((PyInteger stationID, PyList itemIDs) in validIDsByStationID.GetEnumerable <PyInteger, PyList> ())
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

            // extra situation, the repair is happening on a item in our node, the client must know immediately
            if (entry.NodeID == call.MachoNet.NodeID || SystemManager.StationBelongsToUs (entry.LocationID))
            {
                ItemEntity item = ItemFactory.LoadItem (entry.ItemID, out bool loadRequired);

                // the item is an inventory, take everything out!
                if (item is ItemInventory inventory)
                    foreach ((int _, ItemEntity itemInInventory) in inventory.Items)
                        // if the item is in a rig slot, destroy it
                        if (itemInInventory.IsInRigSlot ())
                        {
                            Flags oldFlag = itemInInventory.Flag;
                            ItemFactory.DestroyItem (itemInInventory);
                            // notify the client about the change
                            Dogma.QueueMultiEvent (characterID, OnItemChange.BuildLocationChange (itemInInventory, oldFlag, entry.ItemID));
                        }
                        else
                        {
                            Flags oldFlag = itemInInventory.Flag;
                            // update item's location
                            itemInInventory.LocationID = entry.LocationID;
                            itemInInventory.Flag       = Flags.Hangar;

                            // notify the client about the change
                            Dogma.QueueMultiEvent (characterID, OnItemChange.BuildLocationChange (itemInInventory, oldFlag, entry.ItemID));
                            // save the item
                            itemInInventory.Persist ();
                        }

                // update the singleton flag too
                item.Singleton = false;
                Dogma.QueueMultiEvent (characterID, OnItemChange.BuildSingletonChange (item, true));

                // load was required, the item is not needed anymore
                if (loadRequired)
                    ItemFactory.UnloadItem (item);
            }
            else
            {
                long nodeID = SystemManager.GetNodeStationBelongsTo (entry.LocationID);

                if (nodeID > 0)
                {
                    Notifications.Nodes.Inventory.OnItemChange change = new Notifications.Nodes.Inventory.OnItemChange ();

                    change.AddChange (entry.ItemID, "singleton", true, false);

                    Notifications.NotifyNode (nodeID, change);
                }
            }

            // finally repackage the item
            RepairDB.RepackageItem (entry.ItemID, entry.LocationID);
            // remove any insurance contract for the ship
            InsuranceDB.UnInsureShip (entry.ItemID);
        }

        return null;
    }

    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        if (SystemManager.StationBelongsToUs (parameters.ObjectID))
            return BoundServiceManager.MachoNet.NodeID;

        return SystemManager.GetNodeStationBelongsTo (parameters.ObjectID);
    }

    protected override BoundService CreateBoundInstance (ServiceBindParams bindParams, CallInformation call)
    {
        if (this.MachoResolveObject (bindParams, call) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        Station station = ItemFactory.GetStaticStation (bindParams.ObjectID);

        if (station.HasService (Service.RepairFacilities) == false)
            throw new CustomError ("This station does not allow for repair facilities services");

        // ensure the player is in this station
        if (station.ID != call.Session.StationID)
            throw new CanOnlyDoInStations ();

        ItemInventory inventory =
            ItemFactory.MetaInventoryManager.RegisterMetaInventoryForOwnerID (station, call.Session.EnsureCharacterIsSelected (), Flags.Hangar);

        return new repairSvc (
            RepairDB, MarketDB, InsuranceDB, Notifications, inventory, ItemFactory, BoundServiceManager,
            WalletManager, Dogma, call.Session
        );
    }
}