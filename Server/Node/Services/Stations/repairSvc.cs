using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.repairSvc;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using Node.Notifications.Client.Inventory;
using Node.Services.Account;
using Node.Services.Inventory;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Stations
{
    public class repairSvc : BoundService
    {
        private const double BASEPRICE_MULTIPLIER_MODULE = 0.0125;
        private const double BASEPRICE_MULTIPLIER_SHIP = 0.000088;
        private ItemFactory ItemFactory { get; }
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        private TypeManager TypeManager => this.ItemFactory.TypeManager;
        private ItemInventory mInventory;
        private MarketDB MarketDB { get; }
        private RepairDB RepairDB { get; }
        private InsuranceDB InsuranceDB { get; }
        private NotificationManager NotificationManager { get; }
        private NodeContainer Container { get; }
        private WalletManager WalletManager { get; }

        public repairSvc(RepairDB repairDb, MarketDB marketDb, InsuranceDB insuranceDb, NodeContainer nodeContainer, NotificationManager notificationManager, ItemFactory itemFactory, BoundServiceManager manager, WalletManager walletManager) : base(manager, null)
        {
            this.ItemFactory = itemFactory;
            this.MarketDB = marketDb;
            this.RepairDB = repairDb;
            this.InsuranceDB = insuranceDb;
            this.NotificationManager = notificationManager;
            this.Container = nodeContainer;
            this.WalletManager = walletManager;
        }
        
        protected repairSvc(RepairDB repairDb, MarketDB marketDb, InsuranceDB insuranceDb, NodeContainer nodeContainer, NotificationManager notificationManager, ItemInventory inventory, ItemFactory itemFactory, BoundServiceManager manager, WalletManager walletManager, Client client) : base(manager, client)
        {
            this.mInventory = inventory;
            this.ItemFactory = itemFactory;
            this.MarketDB = marketDb;
            this.RepairDB = repairDb;
            this.InsuranceDB = insuranceDb;
            this.NotificationManager = notificationManager;
            this.Container = nodeContainer;
            this.WalletManager = walletManager;
        }

        public override PyInteger MachoResolveObject(PyInteger stationID, PyInteger zero, CallInformation call)
        {
            // TODO: CHECK IF THE GIVEN STATION HAS REPAIR SERVICES!
            
            if (this.SystemManager.StationBelongsToUs(stationID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeStationBelongsTo(stationID);
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            if (objectData is PyInteger == false)
                throw new CustomError("Cannot bind repairSvc service to unknown object");

            PyInteger stationID = objectData as PyInteger;
            
            if (this.MachoResolveObject(stationID, 0, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            Station station = this.ItemFactory.GetStaticStation(stationID);

            // check if the station has the required services
            if (station.HasService(StaticData.Inventory.Station.Service.RepairFacilities) == false)
                throw new CustomError("This station does not allow for reprocessing plant services");
            // ensure the player is in this station
            if (station.ID != call.Client.StationID)
                throw new CanOnlyDoInStations();
            
            ItemInventory inventory = this.ItemFactory.MetaInventoryManager.RegisterMetaInventoryForOwnerID(station, call.Client.EnsureCharacterIsSelected());

            return new repairSvc(this.RepairDB, this.MarketDB, this.InsuranceDB, this.Container, this.NotificationManager, inventory, this.ItemFactory, this.BoundServiceManager, this.WalletManager, call.Client);
        }

        public PyDataType GetDamageReports(PyList itemIDs, CallInformation call)
        {
            PyDictionary<PyInteger, PyDataType> response = new PyDictionary<PyInteger, PyDataType>();
            
            foreach (PyInteger itemID in itemIDs.GetEnumerable<PyInteger>())
            {
                // ensure the given item is in the list
                if (this.mInventory.Items.TryGetValue(itemID, out ItemEntity item) == false)
                    continue;

                Rowset quote = new Rowset(
                    new PyList<PyString>(6)
                    {
                        "itemID", "typeID", "groupID", "damage", "maxHealth", "costToRepairOneUnitOfDamage"
                    }
                );
                
                if (item is Ship ship)
                {
                    foreach ((int _, ItemEntity module) in ship.Items)
                    {
                        if (module.IsInModuleSlot() == false && module.IsInRigSlot() == false)
                            continue;

                        quote.Rows.Add(
                            new PyList()
                            {
                                module.ID,
                                module.Type.ID,
                                module.Type.Group.ID,
                                module.Attributes[Attributes.damage],
                                module.Attributes[Attributes.hp],
                                // modules should calculate this value differently, but for now this will suffice
                                module.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE
                            }
                        );
                    }
                    
                    
                    quote.Rows.Add(
                        new PyList()
                        {
                            item.ID,
                            item.Type.ID,
                            item.Type.Group.ID,
                            item.Attributes[Attributes.damage],
                            item.Attributes[Attributes.hp],
                            item.Type.BasePrice * BASEPRICE_MULTIPLIER_SHIP
                        }
                    );
                }
                else
                {
                    quote.Rows.Add(
                        new PyList()
                        {
                            item.ID,
                            item.Type.ID,
                            item.Type.Group.ID,
                            item.Attributes[Attributes.damage],
                            item.Attributes[Attributes.hp],
                            item.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE
                        }
                    );
                }

                // the client used to send a lot of extra information on this call
                // but in reality that data is not used by the client at all
                // most likely remnants of older eve client versions
                response[itemID] = new Row(
                    new PyList<PyString>(1) {[0] = "quote"},
                    new PyList(1) {[0] = quote}
                );
            }
            
            return response;
        }

        public PyDataType RepairItems(PyList itemIDs, PyDecimal iskRepairValue, CallInformation call)
        {
            // ensure the player has enough balance to do the fixing
            Station station = this.ItemFactory.GetStaticStation(call.Client.EnsureCharacterIsInStation());

            // take the wallet lock and ensure the character has enough balance
            using Wallet wallet = this.WalletManager.AcquireWallet(call.Client.EnsureCharacterIsSelected(), 1000);
            {
                wallet.EnsureEnoughBalance(iskRepairValue);
                // build a list of items to be fixed
                List<ItemEntity> items = new List<ItemEntity>();

                double quantityLeft = iskRepairValue;
                
                foreach (PyInteger itemID in itemIDs.GetEnumerable<PyInteger>())
                {
                    // ensure the given item is in the list
                    if (this.mInventory.Items.TryGetValue(itemID, out ItemEntity item) == false)
                        continue;

                    // calculate how much to fix it
                    if (item is Ship)
                        quantityLeft -= Math.Min(item.Attributes[Attributes.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_SHIP), quantityLeft);
                    else
                        quantityLeft -= Math.Min(item.Attributes[Attributes.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE), quantityLeft);

                    // add the item to the list
                    items.Add(item);
                    
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
                        repairPrice = item.Attributes[Attributes.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_SHIP);
                    else
                        repairPrice = item.Attributes[Attributes.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE);

                    // full item can be repaired!
                    if (repairPrice <= quantityLeft)
                    {
                        item.Attributes[Attributes.damage].Integer = 0;
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
                            item.Attributes[Attributes.damage] -= repairUnits;
                    }

                    quantityLeft -= repairPrice;
                    // persist item changes
                    item.Persist();
                }
                
                wallet.CreateJournalRecord(MarketReference.RepairBill, station.OwnerID, null, -(iskRepairValue - quantityLeft));
            }

            return null;
        }
        
        public PyDataType UnasembleItems(PyDictionary validIDsByStationID, PyList skipChecks, CallInformation call)
        {
            int characterID = call.Client.EnsureCharacterIsSelected();
            List<RepairDB.ItemRepackageEntry> entries = new List<RepairDB.ItemRepackageEntry>();

            bool ignoreContractVoiding = false;
            bool ignoreRepackageWithUpgrades = false;
            
            foreach (PyString check in skipChecks.GetEnumerable<PyString>())
            {
                if (check == "RepairUnassembleVoidsContract")
                    ignoreContractVoiding = true;
                if (check == "ConfirmRepackageSomethingWithUpgrades")
                    ignoreRepackageWithUpgrades = true;
            }

            foreach ((PyInteger stationID, PyList itemIDs) in validIDsByStationID.GetEnumerable<PyInteger, PyList>())
            {
                foreach (PyInteger itemID in itemIDs.GetEnumerable<PyInteger>())
                {
                    RepairDB.ItemRepackageEntry entry = this.RepairDB.GetItemToRepackage(itemID, characterID, stationID);

                    if (entry.HasContract == true && ignoreContractVoiding == false)
                        throw new RepairUnassembleVoidsContract(this.ItemFactory.TypeManager[entry.TypeID]);
                    if (entry.HasUpgrades == true && ignoreRepackageWithUpgrades == false)
                        throw new ConfirmRepackageSomethingWithUpgrades();
                    if (entry.Damage != 0.0)
                        throw new CantRepackageDamagedItem();

                    entries.Add(entry);
                }
            }

            foreach (RepairDB.ItemRepackageEntry entry in entries)
            {
                if (entry.Singleton == false)
                    continue;
                
                // extra situation, the repair is happening on a item in our node, the client must know immediately
                if (entry.NodeID == this.Container.NodeID || this.SystemManager.StationBelongsToUs(entry.LocationID) == true)
                {
                    ItemEntity item = this.ItemFactory.LoadItem(entry.ItemID, out bool loadRequired);

                    // the item is an inventory, take everything out!
                    if (item is ItemInventory inventory)
                    {
                        foreach ((int _, ItemEntity itemInInventory) in inventory.Items)
                        {
                            // if the item is in a rig slot, destroy it
                            if (itemInInventory.IsInRigSlot() == true)
                            {
                                Flags oldFlag = itemInInventory.Flag;
                                this.ItemFactory.DestroyItem(itemInInventory);
                                // notify the client about the change
                                call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(itemInInventory, oldFlag, entry.ItemID));
                            }
                            else
                            {
                                Flags oldFlag = itemInInventory.Flag;
                                // update item's location
                                itemInInventory.LocationID = entry.LocationID;
                                itemInInventory.Flag = Flags.Hangar;
                            
                                // notify the client about the change
                                call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(itemInInventory, oldFlag, entry.ItemID));
                                // save the item
                                itemInInventory.Persist();
                            }
                        }
                    }
                    
                    // update the singleton flag too
                    item.Singleton = false;
                    call.Client.NotifyMultiEvent(OnItemChange.BuildSingletonChange(item, true));

                    // load was required, the item is not needed anymore
                    if (loadRequired == true)
                    {
                        this.ItemFactory.UnloadItem(item);
                    }
                }
                else
                {
                    long nodeID = this.SystemManager.GetNodeStationBelongsTo(entry.LocationID);

                    if (nodeID > 0)
                    {
                        Notifications.Nodes.Inventory.OnItemChange change = new Notifications.Nodes.Inventory.OnItemChange();

                        change.AddChange(entry.ItemID, "singleton", true, false);
                        
                        this.NotificationManager.NotifyNode(nodeID, change);
                    }
                }
                
                // finally repackage the item
                this.RepairDB.RepackageItem(entry.ItemID, entry.LocationID);
                // remove any insurance contract for the ship
                this.InsuranceDB.UnInsureShip(entry.ItemID);
            }
            
            return null;
        }
    }
}