using Node.Inventory.Items;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Inventory
{
    public class invbroker : BoundService
    {
        private int mObjectID;
        
        public invbroker(ServiceManager manager) : base(manager)
        {
        }

        private invbroker(ServiceManager manager, int objectID) : base(manager)
        {
            this.mObjectID = objectID;
        }

        protected override Service CreateBoundInstance(PyDataType objectData)
        {
            /*
             * objectData[0] => itemID (station/solarsystem)
             * objectData[1] => itemGroup
             */
            PyTuple tupleData = objectData as PyTuple;
            
            return new invbroker(this.ServiceManager, tupleData[0] as PyInteger);
        }

        public PyDataType GetInventoryFromId(PyInteger itemID, PyInteger one, PyDictionary namedPayload, Client client)
        {
            ItemEntity inventoryItem = this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem(itemID);

            // ensure the itemID is owned by the client's character
            if (inventoryItem.OwnerID != client.CharacterID && inventoryItem.ID != client.CharacterID)
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
            return BoundInventory.BindInventory(inventoryItem as ItemInventory, ItemFlags.None, this.ServiceManager);
        }

        public PyDataType GetInventory(PyInteger containerID, PyNone none, PyDictionary namedPayload, Client client)
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
            
            ItemEntity inventoryItem = this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem(this.mObjectID);
            
            // create an instance of the inventory service and bind it to the item data
            return BoundInventory.BindInventory(inventoryItem as ItemInventory, flag, this.ServiceManager);
        }
    }
}