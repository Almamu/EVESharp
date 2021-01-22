using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.Logging;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Inventory
{
    public class BoundInventory : BoundService
    {
        private ItemInventory mInventory;
        private ItemFlags mFlag;
        private ItemDB ItemDB { get; }
        private NodeContainer NodeContainer { get; }
        private ItemManager ItemManager { get; }

        public BoundInventory(ItemDB itemDB, ItemInventory item, ItemManager itemManager, NodeContainer nodeContainer, BoundServiceManager manager) : base(manager)
        {
            this.mInventory = item;
            this.mFlag = ItemFlags.None;
            this.ItemDB = itemDB;
            this.ItemManager = itemManager;
            this.NodeContainer = nodeContainer;
        }

        public BoundInventory(ItemDB itemDB, ItemInventory item, ItemFlags flag, ItemManager itemManager, NodeContainer nodeContainer, BoundServiceManager manager) : base(manager)
        {
            this.mInventory = item;
            this.mFlag = flag;
            this.ItemDB = itemDB;
            this.ItemManager = itemManager;
            this.NodeContainer = nodeContainer;
        }

        public PyDataType List(CallInformation call)
        {        
            CRowset result = new CRowset(ItemEntity.sEntityItemDescriptor);

            foreach (KeyValuePair<int, ItemEntity> pair in this.mInventory.Items)
                if (pair.Value.Flag == this.mFlag)
                    result.Add(pair.Value.GetEntityRow());

            return result;
        }

        public PyDataType ListStations(PyInteger blueprintsOnly, PyInteger forCorp, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            // TODO: take into account blueprintsOnly
            if (forCorp == 1)
                return this.ItemDB.ListStations(call.Client.CorporationID);
            else
                return this.ItemDB.ListStations(callerCharacterID);
        }

        public PyDataType ListStationItems(PyInteger stationID, CallInformation call)
        {
            return this.ItemDB.ListStationItems(stationID, call.Client.EnsureCharacterIsSelected());
        }
        
        public PyDataType GetItem(CallInformation call)
        {
            return this.mInventory.GetEntityRow();
        }

        public PyDataType Add(PyInteger itemID, PyInteger quantity, PyInteger flag, CallInformation call)
        {
            // TODO: PROPERLY INVESTIGATE THE ADD FUNCTION
            // TODO: DOES IT GET CALLED FOR ITEMS ON DIFFERENT INVENTORIES?
            // TODO: OR WHAT DOES EXACTLY HAPPEN WITH THIS?
            if (this.mInventory.Items.ContainsKey(itemID) == false)
                return null;
            
            // for now treat the item as it comes from the same inventory and we're just splitting the quantities
            ItemEntity item = this.mInventory.Items[itemID];

            // ensure there's enough quantity in the stack to split it
            if (quantity >= item.Quantity)
                return null;

            // create a new item with the same specs as the original
            ItemEntity clone = this.ItemManager.CreateSimpleItem(item.Type, item.OwnerID, item.LocationID, this.mFlag, quantity,
                item.Contraband, item.Singleton);
            
            // subtract the quantity off the original item
            item.Quantity -= quantity;
            // notify the changes to the client
            call.Client.NotifyItemQuantityChange(item, item.Quantity + quantity);
            call.Client.NotifyItemLocationChange(clone, ItemFlags.None, 0);
            // add the new item to the inventory
            this.mInventory.AddItem(clone);
            
            return null;
        }

        public PyDataType MultiMerge(PyList merges, CallInformation call)
        {
            foreach (PyDataType merge in merges)
            {
                // ignore wrong multimerge formats
                if (merge is PyTuple == false)
                    continue;

                PyTuple tuple = merge as PyTuple;

                if (tuple[0] is PyInteger == false || tuple[1] is PyInteger == false || tuple[2] is PyInteger == false)
                    continue;

                PyInteger fromItemID = tuple[0] as PyInteger;
                PyInteger toItemID = tuple[1] as PyInteger;
                PyInteger quantity = tuple[2] as PyInteger;

                if (this.mInventory.Items.ContainsKey(fromItemID) == false ||
                    this.mInventory.Items.ContainsKey(toItemID) == false)
                    continue;

                ItemEntity fromItem = this.mInventory.Items[fromItemID];
                ItemEntity toItem = this.mInventory.Items[toItemID];

                // ignore singleton items
                if (fromItem.Singleton == true || toItem.Singleton == true)
                    continue;
                
                // if we're fully merging two stacks, just remove one item
                if (quantity == fromItem.Quantity)
                {
                    // remove the item
                    fromItem.Destroy();
                    // update the item to something else so the item is take out of player's sight
                    fromItem.LocationID = this.NodeContainer.Constants["locationRecycler"];
                    // notify the client about the item too
                    call.Client.NotifyItemLocationChange(fromItem, fromItem.Flag, toItem.LocationID);
                }
                else
                {
                    // change the item's quantity
                    fromItem.Quantity -= quantity;
                    // notify the client about the change
                    call.Client.NotifyItemQuantityChange(fromItem, fromItem.Quantity + quantity);
                }

                toItem.Quantity += quantity;
                call.Client.NotifyItemQuantityChange(toItem, toItem.Quantity - quantity);
            }
            
            return null;
        }

        public PyDataType StackAll(PyString password, CallInformation call)
        {
            throw new NotImplementedException("Stacking on passworded containers is not supported yet!");
        }

        public PyDataType StackAll(PyInteger locationFlag, CallInformation call)
        {
            // figure out exactly what this does
            // it could be used mostly on ship fittings
            throw new NotImplementedException("Stacking by flag is not supported yet!");
        }
        
        public PyDataType StackAll(CallInformation call)
        {
            foreach (int firstItemID in this.mInventory.Items.Keys)
            {
                ItemEntity firstItem = this.mInventory.Items[firstItemID];
                
                // singleton items are not even checked
                if (firstItem.Singleton == true)
                    continue;
                
                foreach (int secondItemID in this.mInventory.Items.Keys)
                {
                    // ignore the same itemID as they cannot really be merged
                    if (firstItemID == secondItemID)
                        continue;
                    
                    ItemEntity secondItem = this.mInventory.Items[secondItemID];
                    
                    // ignore the item if it's singleton
                    if (secondItem.Singleton == true)
                        continue;
                    // ignore the item check if they're not the same type ID
                    if (firstItem.Type.ID != secondItem.Type.ID)
                        continue;
                    int oldQuantity = secondItem.Quantity;
                    // add the quantity of the first item to the second
                    secondItem.Quantity += firstItem.Quantity;
                    // also create the notification for the user
                    call.Client.NotifyItemQuantityChange(secondItem, oldQuantity);
                    // remove the item off the list
                    this.mInventory.Items.Remove(firstItemID);
                    // update the item to something else so the item is take out of player's sight
                    firstItem.LocationID = this.NodeContainer.Constants["locationRecycler"];
                    // notify the client about the item too
                    call.Client.NotifyItemLocationChange(firstItem, firstItem.Flag, secondItem.LocationID);
                    // delete the original item off the database
                    firstItem.Destroy();
                    // ensure the second item is saved to database too
                    secondItem.Persist();
                    // finally break this loop as the merge was already done
                    break;
                }
            }

            return null;
        }

        public static PyDataType BindInventory(ItemDB itemDB, ItemInventory item, ItemFlags flag, ItemManager itemManager, NodeContainer nodeContainer, BoundServiceManager boundServiceManager)
        {
            BoundService instance = new BoundInventory(itemDB, item, flag, itemManager, nodeContainer, boundServiceManager);
            // bind the service
            int boundID = boundServiceManager.BoundService(instance);
            // build the bound service string
            string boundServiceStr = boundServiceManager.BuildBoundServiceString(boundID);

            // TODO: the expiration time is 1 day, might be better to properly support this?
            // TODO: investigate these a bit more closely in the future
            // TODO: i'm not so sure about the expiration time
            PyTuple boundServiceInformation = new PyTuple(new PyDataType[]
            {
                boundServiceStr, DateTime.UtcNow.Add(TimeSpan.FromDays(1)).ToFileTime()
            });

            // after the service is bound the call can be run (if required)
            return new PySubStruct(new PySubStream(boundServiceInformation));
        }
    }
}