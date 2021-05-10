using EVE;
using EVE.Packets.Exceptions;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.corpRegistry;
using Node.Exceptions.inventory;
using Node.Exceptions.ship;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.Notifications.Client.Inventory;
using Node.StaticData.Corporation;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;
using Container = Node.StaticData.Inventory.Container;

namespace Node.Services.Inventory
{
    public class invbroker : BoundService
    {
        private int mObjectID;
        
        private ItemFactory ItemFactory { get; }
        private ItemDB ItemDB { get; }
        private NodeContainer NodeContainer { get; }
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        private NotificationManager NotificationManager { get; init; }

        public invbroker(ItemDB itemDB, ItemFactory itemFactory, NodeContainer nodeContainer, NotificationManager notificationManager, BoundServiceManager manager) : base(manager, null)
        {
            this.ItemFactory = itemFactory;
            this.ItemDB = itemDB;
            this.NodeContainer = nodeContainer;
            this.NotificationManager = notificationManager;
        }

        private invbroker(ItemDB itemDB, ItemFactory itemFactory, NodeContainer nodeContainer, NotificationManager notificationManager, BoundServiceManager manager, int objectID, Client client) : base(manager, client)
        {
            this.ItemFactory = itemFactory;
            this.ItemDB = itemDB;
            this.mObjectID = objectID;
            this.NodeContainer = nodeContainer;
            this.NotificationManager = notificationManager;
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

            if (groupID == (int) Groups.SolarSystem)
                solarSystemID = this.ItemFactory.GetStaticSolarSystem(entityID).ID;
            else if (groupID == (int) Groups.Station)
                solarSystemID = this.ItemFactory.GetStaticStation(entityID).SolarSystemID;
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
            
            return new invbroker(this.ItemDB, this.ItemFactory, this.NodeContainer, this.NotificationManager, this.BoundServiceManager, tupleData[0] as PyInteger, call.Client);
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

        private PySubStruct BindInventory(ItemInventory inventoryItem, int ownerID, Client client, Flags flag)
        {
            ItemInventory inventory = inventoryItem;
            
            // create a meta inventory only if required
            if (inventoryItem is not Ship && inventoryItem is not Character)
                inventory = this.ItemFactory.MetaInventoryManager.RegisterMetaInventoryForOwnerID(inventoryItem, ownerID, flag);
            
            // create an instance of the inventory service and bind it to the item data
            return BoundInventory.BindInventory(this.ItemDB, inventory, flag, this.ItemFactory, this.NodeContainer, this.NotificationManager, this.BoundServiceManager, client);
        }

        public PySubStruct GetInventoryFromId(PyInteger itemID, PyInteger one, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            ItemEntity inventoryItem = this.ItemFactory.LoadItem(itemID);

            return this.BindInventory(
                this.CheckInventoryBeforeLoading(inventoryItem),
                callerCharacterID,
                call.Client, Flags.None
            );
        }

        public PySubStruct GetInventory(PyInteger containerID, PyDataType none, CallInformation call)
        {
            int ownerID = call.Client.EnsureCharacterIsSelected();
            
            Flags flag = Flags.None;
            
            switch ((int) containerID)
            {
                case (int) Container.Wallet:
                    flag = Flags.Wallet;
                    break;
                case (int) Container.Hangar:
                    flag = Flags.Hangar;
                    break;
                case (int) Container.Character:
                    flag = Flags.Skill;
                    break;
                case (int) Container.Global:
                    flag = Flags.None;
                    break;
                case (int) Container.CorpMarket:
                    flag = Flags.CorpMarket;
                    ownerID = call.Client.CorporationID;
                    
                    // check permissions
                    if (CorporationRole.Accountant.Is(call.Client.CorporationRole) == false &&
                        CorporationRole.JuniorAccountant.Is(call.Client.CorporationRole) == false &&
                        CorporationRole.Trader.Is(call.Client.CorporationRole) == false)
                        throw new CrpAccessDenied(MLS.UI_CORP_ACCESSDENIED14);
                    break;
                
                default:
                    throw new CustomError($"Trying to open container ID ({containerID.Value}) is not supported");
            }
            
            // get the inventory item first
            ItemEntity inventoryItem = this.ItemFactory.LoadItem(this.mObjectID);

            return this.BindInventory(
                this.CheckInventoryBeforeLoading(inventoryItem),
                ownerID,
                call.Client, flag
            );
        }

        public PyDataType TrashItems(PyList itemIDs, PyInteger stationID, CallInformation call)
        {
            foreach (PyInteger itemID in itemIDs.GetEnumerable<PyInteger>())
            {
                // do not trash the active ship
                if (itemID == call.Client.ShipID)
                    throw new CantMoveActiveShip();

                ItemEntity item = this.ItemFactory.GetItem(itemID);
                // store it's location id
                int oldLocation = item.LocationID;
                Flags oldFlag = item.Flag;
                // remove the item off the ItemManager
                this.ItemFactory.DestroyItem(item);
                // notify the client of the change
                call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(item, oldFlag, oldLocation));
                // TODO: CHECK IF THE ITEM HAS ANY META INVENTORY AND/OR BOUND SERVICE
                // TODO: AND FREE THOSE TOO SO THE ITEMS CAN BE REMOVED OFF THE DATABASE
            }
            
            return null;
        }
        
        public PyDataType SetLabel(PyInteger itemID, PyString newLabel, CallInformation call)
        {
            ItemEntity item = this.ItemFactory.GetItem(itemID);

            // ensure the itemID is owned by the client's character
            if (item.OwnerID != call.Client.EnsureCharacterIsSelected())
                throw new TheItemIsNotYoursToTake(itemID);

            item.Name = newLabel;
            
            // ensure the item is saved into the database first
            item.Persist();
            
            // notify the owner of the item
            call.Client.NotifyMultiEvent(OnCfgDataChanged.BuildItemLabelChange(item));

            // TODO: CHECK IF ITEM BELONGS TO CORP AND NOTIFY CHARACTERS IN THIS NODE?
            return null;
        }

        public PyDataType AssembleCargoContainer(PyInteger containerID, PyDataType ignored, PyDecimal ignored2,
            CallInformation call)
        {
            ItemEntity item = this.ItemFactory.GetItem(containerID);

            if (item.OwnerID != call.Client.EnsureCharacterIsSelected())
                throw new TheItemIsNotYoursToTake(containerID);
            
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