using Node.Database;
using Node.Exceptions;
using Node.Exceptions.ship;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Inventory.Notifications;
using Node.Network;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Inventory
{
    public class invbroker : BoundService
    {
        private int mObjectID;
        
        private ItemManager ItemManager { get; }
        private ItemDB ItemDB { get; }
        private NodeContainer NodeContainer { get; }
        private SystemManager SystemManager { get; }

        public invbroker(ItemDB itemDB, ItemManager itemManager, NodeContainer nodeContainer, SystemManager systemManager, BoundServiceManager manager) : base(manager, null)
        {
            this.ItemManager = itemManager;
            this.ItemDB = itemDB;
            this.NodeContainer = nodeContainer;
            this.SystemManager = systemManager;
        }

        private invbroker(ItemDB itemDB, ItemManager itemManager, NodeContainer nodeContainer, SystemManager systemManager, BoundServiceManager manager, int objectID, Client client) : base(manager, client)
        {
            this.ItemManager = itemManager;
            this.ItemDB = itemDB;
            this.mObjectID = objectID;
            this.NodeContainer = nodeContainer;
            this.SystemManager = systemManager;
        }

        public override PyInteger MachoResolveObject(PyTuple objectData, PyInteger zero, CallInformation call)
        {
            /*
             * objectData [0] => entityID (station or solar system)
             * objectData [1] => groupID (station or solar system)
             */

            PyDataType first = objectData[0];
            PyDataType second = objectData[1];

            if (first is PyInteger == false || second is PyInteger == false)
                throw new CustomError("Cannot resolve object");

            PyInteger entityID = first as PyInteger;
            PyInteger groupID = second as PyInteger;

            int solarSystemID = 0;

            if (groupID == (int) ItemGroups.SolarSystem)
                solarSystemID = this.ItemManager.GetSolarSystem(entityID).ID;
            else if (groupID == (int) ItemGroups.Station)
                solarSystemID = this.ItemManager.GetStation(entityID).SolarSystemID;
            else
                throw new CustomError("Unknown item's groupID");

            if (this.SystemManager.SolarSystemBelongsToUs(solarSystemID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeSolarSystemBelongsTo(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            /*
             * objectData[0] => itemID (station/solarsystem)
             * objectData[1] => itemGroup
             */
            PyTuple tupleData = objectData as PyTuple;
            
            if (this.MachoResolveObject(tupleData, 0, call) != this.NodeContainer.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");
            
            return new invbroker(this.ItemDB, this.ItemManager, this.NodeContainer, this.SystemManager, this.BoundServiceManager, tupleData[0] as PyInteger, call.Client);
        }

        private ItemInventory CheckInventoryBeforeLoading(ItemEntity inventoryItem)
        {
            // also make sure it's a container
            if (inventoryItem is ItemInventory == false)
                throw new ItemNotContainer(inventoryItem.ID);
            
            // extra check, ensure it's a singleton if not a station
            if (inventoryItem is Station == false && inventoryItem.Singleton == false)
                throw new AssembleCCFirst();
            
            return inventoryItem as ItemInventory;
        }

        private PyDataType BindInventory(ItemInventory inventoryItem, int characterID, Client client, ItemFlags flag)
        {
            // build the meta inventory item now
            ItemInventory inventoryByOwner = this.ItemManager.MetaInventoryManager.RegisterMetaInventoryForOwnerID(inventoryItem,
                characterID);
            
            // create an instance of the inventory service and bind it to the item data
            return BoundInventory.BindInventory(this.ItemDB, inventoryByOwner, flag, this.ItemManager, this.NodeContainer, this.BoundServiceManager, client);
        }

        public PyDataType GetInventoryFromId(PyInteger itemID, PyInteger one, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            ItemEntity inventoryItem = this.ItemManager.LoadItem(itemID);

            return this.BindInventory(
                this.CheckInventoryBeforeLoading(inventoryItem),
                callerCharacterID,
                call.Client, ItemFlags.None
            );
        }

        public PyDataType GetInventory(PyInteger containerID, PyNone none, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            ItemFlags flag = ItemFlags.None;
            
            switch ((int) containerID)
            {
                case (int) ItemContainer.Wallet:
                    flag = ItemFlags.Wallet;
                    break;
                case (int) ItemContainer.Hangar:
                    flag = ItemFlags.Hangar;
                    break;
                case (int) ItemContainer.Character:
                    flag = ItemFlags.Skill;
                    break;
                case (int) ItemContainer.Global:
                    flag = ItemFlags.None;
                    break;
                
                default:
                    throw new CustomError($"Trying to open container ID ({containerID.Value}) is not supported");
            }
            
            // get the inventory item first
            ItemEntity inventoryItem = this.ItemManager.LoadItem(this.mObjectID);

            return this.BindInventory(
                this.CheckInventoryBeforeLoading(inventoryItem),
                callerCharacterID,
                call.Client, flag
            );
        }

        public PyDataType TrashItems(PyList itemIDs, PyInteger stationID, CallInformation call)
        {
            foreach (PyDataType itemID in itemIDs)
            {
                // ignore non integer values and the current
                if (itemID is PyInteger integer == false)
                    continue;
                PyInteger value = itemID as PyInteger;

                // do not trash the active ship
                if (value == call.Client.ShipID)
                    throw new CantMoveActiveShip();

                ItemEntity item = this.ItemManager.GetItem(itemID as PyInteger);
                // store it's location id
                int oldLocation = item.LocationID;
                ItemFlags oldFlag = item.Flag;
                // remove the item off the ItemManager
                this.ItemManager.DestroyItem(item);
                // notify the client of the change
                call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(item, oldFlag, oldLocation));
            }
            
            return null;
        }
        
        public PyDataType SetLabel(PyInteger itemID, PyString newLabel, CallInformation call)
        {
            ItemEntity item = this.ItemManager.GetItem(itemID);

            // ensure the itemID is owned by the client's character
            if (item.OwnerID != call.Client.EnsureCharacterIsSelected())
                throw new TheItemIsNotYoursToTake(itemID);

            item.Name = newLabel;
            
            // ensure the item is saved into the database first
            item.Persist();
            
            // if the item is a ship, send a session change
            if (item.Type.Group.Category.ID == (int) ItemCategories.Ship)
                call.Client.ShipID = call.Client.ShipID;

            // TODO: CHECK IF ITEM BELONGS TO CORP AND NOTIFY CHARACTERS IN THIS NODE?
            return null;
        }

        public PyDataType AssembleCargoContainer(PyInteger containerID, PyNone ignored, PyDecimal ignored2,
            CallInformation call)
        {
            ItemEntity item = this.ItemManager.GetItem(containerID);

            if (item.OwnerID != call.Client.EnsureCharacterIsSelected())
                throw new TheItemIsNotYoursToTake(containerID);
            
            // ensure the item is a cargo container
            switch (item.Type.Group.ID)
            {
                case (int) ItemGroups.CargoContainer:
                case (int) ItemGroups.SecureCargoContainer:
                case (int) ItemGroups.AuditLogSecureContainer:
                case (int) ItemGroups.FreightContainer:
                case (int) ItemGroups.Tool:
                case (int) ItemGroups.MobileWarpDisruptor:
                    break;
                default:
                    throw new ItemNotContainer(containerID);
            }

            bool oldSingleton = item.Singleton;
            
            // update singleton
            item.Singleton = true;
            item.Persist();

            // notify the client
            call.Client.NotifyMultiEvent(OnItemChange.BuildSingletonChange(item, oldSingleton));

            return null;
        }
    }
}