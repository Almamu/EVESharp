using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Node.Database;
using Node.Exceptions;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using Node.Services.Inventory;
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
        private ItemManager ItemManager => this.ItemFactory.ItemManager;
        private SystemManager SystemManager { get; }
        private TypeManager TypeManager => this.ItemFactory.TypeManager;
        private ItemInventory mInventory;
        private MarketDB MarketDB { get; }

        public repairSvc(MarketDB marketDb, ItemFactory itemFactory, SystemManager systemManager, BoundServiceManager manager) : base(manager, null)
        {
            this.ItemFactory = itemFactory;
            this.SystemManager = systemManager;
            this.MarketDB = marketDb;
        }
        
        protected repairSvc(MarketDB marketDb, ItemInventory inventory, ItemFactory itemFactory, SystemManager systemManager, BoundServiceManager manager, Client client) : base(manager, client)
        {
            this.mInventory = inventory;
            this.ItemFactory = itemFactory;
            this.SystemManager = systemManager;
            this.MarketDB = marketDb;
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

            Station station = this.ItemManager.GetStaticStation(stationID);
            
            ItemInventory inventory = this.ItemManager.MetaInventoryManager.RegisterMetaInventoryForOwnerID(station, call.Client.EnsureCharacterIsSelected());

            return new repairSvc(this.MarketDB, inventory, this.ItemFactory, this.SystemManager, this.BoundServiceManager, call.Client);
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
                    new PyList()
                    {
                        "itemID", "typeID", "groupID", "damage", "maxHealth", "costToRepairOneUnitOfDamage"
                    }
                );
                
                if (item is Ship ship)
                {
                    foreach ((int _, ItemEntity module) in ship.Items)
                    {
                        if (module.IsInModuleSlot() == false)
                            continue;

                        quote.Rows.Add(
                            new PyList()
                            {
                                module.ID,
                                module.Type.ID,
                                module.Type.Group.ID,
                                module.Attributes[AttributeEnum.damage],
                                module.Attributes[AttributeEnum.hp],
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
                            item.Attributes[AttributeEnum.damage],
                            item.Attributes[AttributeEnum.hp],
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
                            item.Attributes[AttributeEnum.damage],
                            item.Attributes[AttributeEnum.hp],
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
            Character character = this.ItemManager.GetItem<Character>(call.Client.EnsureCharacterIsSelected());
            Station station = this.ItemManager.GetStaticStation(call.Client.EnsureCharacterIsInStation());

            // quick fast check
            if (character.Balance < iskRepairValue)
                throw new NotEnoughMoney(character.Balance, iskRepairValue);
            
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
                    quantityLeft -= Math.Min(item.Attributes[AttributeEnum.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_SHIP), quantityLeft);
                else
                    quantityLeft -= Math.Min(item.Attributes[AttributeEnum.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE), quantityLeft);

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
                    repairPrice = item.Attributes[AttributeEnum.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_SHIP);
                else
                    repairPrice = item.Attributes[AttributeEnum.damage] * (item.Type.BasePrice * BASEPRICE_MULTIPLIER_MODULE);

                // full item can be repaired!
                if (repairPrice <= quantityLeft)
                {
                    item.Attributes[AttributeEnum.damage].Integer = 0;
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
                        item.Attributes[AttributeEnum.damage] -= repairUnits;
                }

                quantityLeft -= repairPrice;
                // persist item changes
                item.Persist();
            }
            
            // change balance
            character.Balance -= (iskRepairValue - quantityLeft);
            // create entry in journal
            this.MarketDB.CreateJournalForCharacter(MarketReference.RepairBill, character.ID, character.ID, station.OwnerID, null, (iskRepairValue - quantityLeft), character.Balance, "", 1000);
            // notify client
            call.Client.NotifyBalanceUpdate(character.Balance);
            
            // notify changes on the damage attribute
            call.Client.NotifyAttributeChange(AttributeEnum.damage, items.ToArray());
            
            return null;
        }
    }
}