using System.Runtime.CompilerServices;
using Common.Logging;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Network;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Inventory
{
    public class invbroker : BoundService
    {
        private int mObjectID;
        
        private ItemManager ItemManager { get; }
        private ItemDB ItemDB { get; }
        
        public invbroker(ItemDB itemDB, ItemManager itemManager, BoundServiceManager manager) : base(manager)
        {
            this.ItemManager = itemManager;
            this.ItemDB = itemDB;
        }

        private invbroker(ItemDB itemDB, ItemManager itemManager, BoundServiceManager manager, int objectID) : base(manager)
        {
            this.ItemManager = itemManager;
            this.ItemDB = itemDB;
            this.mObjectID = objectID;
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            /*
             * objectData[0] => itemID (station/solarsystem)
             * objectData[1] => itemGroup
             */
            PyTuple tupleData = objectData as PyTuple;
            
            return new invbroker(this.ItemDB, this.ItemManager, this.BoundServiceManager, tupleData[0] as PyInteger);
        }

        public PyDataType GetInventoryFromId(PyInteger itemID, PyInteger one, CallInformation call)
        {
            ItemEntity inventoryItem = this.ItemManager.LoadItem(itemID);

            // ensure the itemID is owned by the client's character
            if (inventoryItem.OwnerID != call.Client.CharacterID && inventoryItem.ID != call.Client.CharacterID)
                throw new UserError("TheItemIsNotYoursToTake", new PyDictionary()
                {
                    {"item", itemID}
                });

            // also make sure it's a container
            if (inventoryItem is ItemInventory == false)
                throw new UserError("ItemNotContainer", new PyDictionary()
                {
                    {"itemid", itemID}
                });
            
            // create an instance of the inventory service and bind it to the item data
            return BoundInventory.BindInventory(this.ItemDB, inventoryItem as ItemInventory, ItemFlags.None, this.BoundServiceManager);
        }

        public PyDataType GetInventory(PyInteger containerID, PyNone none, CallInformation call)
        {
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
            
            ItemEntity inventoryItem = this.ItemManager.LoadItem(this.mObjectID);
            
            // create an instance of the inventory service and bind it to the item data
            return BoundInventory.BindInventory(this.ItemDB, inventoryItem as ItemInventory, flag, this.BoundServiceManager);
        }

        public PyDataType SetLabel(PyInteger itemID, PyString newLabel, CallInformation call)
        {
            ItemEntity item = this.ItemManager.LoadItem(itemID);

            // ensure the itemID is owned by the client's character
            if (item.OwnerID != call.Client.CharacterID)
                throw new UserError("TheItemIsNotYoursToTake", new PyDictionary()
                {
                    {"item", itemID}
                });

            item.Name = newLabel;
            
            // ensure the item is saved into the database first
            item.Persist();
            
            // if the item is a ship, send a session change
            if (item.Type.Group.Category.ID == (int) ItemCategories.Ship)
            {
                call.Client.ShipID = call.Client.ShipID;
                
                call.Client.SendSessionChange();
            }

            // TODO: CHECK IF ITEM BELONGS TO CORP AND NOTIFY CHARACTERS IN THIS NODE?
            return null;
        }
    }
}