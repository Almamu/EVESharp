using EVESharp.EVE;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.corpRegistry;
using EVESharp.Node.Exceptions.inventory;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.StaticData.Corporation;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Exceptions;
using EVESharp.Node.Exceptions.ship;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using Container = EVESharp.Node.StaticData.Inventory.Container;

namespace EVESharp.Node.Services.Inventory
{
    public class invbroker : ClientBoundService
    {
        private int mObjectID;
        
        private ItemFactory ItemFactory { get; }
        private ItemDB ItemDB { get; }
        private NodeContainer NodeContainer { get; }
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        private NotificationManager NotificationManager { get; init; }

        public invbroker(ItemDB itemDB, ItemFactory itemFactory, NodeContainer nodeContainer, NotificationManager notificationManager, BoundServiceManager manager) : base(manager)
        {
            this.ItemFactory = itemFactory;
            this.ItemDB = itemDB;
            this.NodeContainer = nodeContainer;
            this.NotificationManager = notificationManager;
        }

        private invbroker(ItemDB itemDB, ItemFactory itemFactory, NodeContainer nodeContainer, NotificationManager notificationManager, BoundServiceManager manager, int objectID, Client client) : base(manager, client, objectID)
        {
            this.ItemFactory = itemFactory;
            this.ItemDB = itemDB;
            this.mObjectID = objectID;
            this.NodeContainer = nodeContainer;
            this.NotificationManager = notificationManager;
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
            int ownerID = call.Client.EnsureCharacterIsSelected();
            ItemEntity inventoryItem = this.ItemFactory.LoadItem(itemID);

            if (inventoryItem is not Station)
                ownerID = inventoryItem.OwnerID;

            return this.BindInventory(
                this.CheckInventoryBeforeLoading(inventoryItem),
                ownerID,
                call.Client, Flags.None
            );
        }

        public PySubStruct GetInventory(PyInteger containerID, PyInteger origOwnerID, CallInformation call)
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

                    if (origOwnerID is not null)
                        ownerID = origOwnerID;

                    if (ownerID != call.Client.CharacterID && CorporationRole.SecurityOfficer.Is(call.Client.CorporationRole) == false)
                        throw new CrpAccessDenied(MLS.UI_CORP_ACCESSDENIED13);
                    
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
            this.NotificationManager.NotifyOwner(item.OwnerID, OnCfgDataChanged.BuildItemLabelChange(item));

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

        public PyDataType DeliverToCorpHangar(PyInteger stationID, PyList itemIDs, PyDataType quantity, PyInteger ownerID, PyInteger deliverToFlag, CallInformation call)
        {
            // TODO: DETERMINE IF THIS FUNCTION HAS TO BE IMPLEMENTED
            // LIVE CCP SERVER DOES NOT SUPPORT IT, EVEN THO THE MENU OPTION IS SHOWN TO THE USER
            return null;
        }

        public PyDataType DeliverToCorpMember(PyInteger memberID, PyInteger stationID, PyList itemIDs, PyDataType quantity, PyInteger ownerID, CallInformation call)
        {
            return null;
        }
        
        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            int solarSystemID = 0;

            if (parameters.ExtraValue == (int) Groups.SolarSystem)
                solarSystemID = this.ItemFactory.GetStaticSolarSystem(parameters.ObjectID).ID;
            else if (parameters.ExtraValue == (int) Groups.Station)
                solarSystemID = this.ItemFactory.GetStaticStation(parameters.ObjectID).SolarSystemID;
            else
                throw new CustomError("Unknown item's groupID");

            if (this.SystemManager.SolarSystemBelongsToUs(solarSystemID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeSolarSystemBelongsTo(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            
            if (this.MachoResolveObject(bindParams, call) != this.NodeContainer.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");
            
            return new invbroker(this.ItemDB, this.ItemFactory, this.NodeContainer, this.NotificationManager, this.BoundServiceManager, bindParams.ObjectID, call.Client);
        }
    }
}