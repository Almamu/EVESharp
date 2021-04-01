using System.Runtime.CompilerServices;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
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
        private ItemFactory ItemFactory { get; }
        private ItemManager ItemManager => this.ItemFactory.ItemManager;
        private SystemManager SystemManager { get; }
        private TypeManager TypeManager => this.ItemFactory.TypeManager;
        private ItemInventory mInventory;

        public repairSvc(ItemFactory itemFactory, SystemManager systemManager, BoundServiceManager manager) : base(manager, null)
        {
            this.ItemFactory = itemFactory;
            this.SystemManager = systemManager;
        }
        
        protected repairSvc(ItemInventory inventory, ItemFactory itemFactory, SystemManager systemManager, BoundServiceManager manager, Client client) : base(manager, client)
        {
            this.mInventory = inventory;
            this.ItemFactory = itemFactory;
            this.SystemManager = systemManager;
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

            return new repairSvc(inventory, this.ItemFactory, this.SystemManager, this.BoundServiceManager, call.Client);
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
                    foreach ((int _, ItemEntity shipItem) in ship.Items)
                    {
                        if (shipItem.IsInModuleSlot() == false)
                            continue;

                        quote.Rows.Add(
                            new PyList()
                            {
                                shipItem.ID,
                                shipItem.Type.ID,
                                shipItem.Type.Group.ID,
                                shipItem.Attributes[AttributeEnum.damage],
                                shipItem.Attributes[AttributeEnum.hp],
                                // modules should calculate this value differently, but for now this will suffice
                                shipItem.Type.BasePrice * 0.000088
                            }
                        );
                    }
                }

                quote.Rows.Add(
                    new PyList()
                    {
                        item.ID,
                        item.Type.ID,
                        item.Type.Group.ID,
                        item.Attributes[AttributeEnum.damage],
                        item.Attributes[AttributeEnum.hp],
                        item.Type.BasePrice * 0.000088
                    }
                );

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
    }
}