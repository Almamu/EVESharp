using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.Logging;
using Node.Database;
using Node.Inventory.Items;
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
        public BoundInventory(ItemDB itemDB, ItemInventory item, BoundServiceManager manager, Logger logger) : base(manager, logger)
        {
            this.mInventory = item;
            this.mFlag = ItemFlags.None;
            this.ItemDB = itemDB;
        }

        public BoundInventory(ItemDB itemDB, ItemInventory item, ItemFlags flag, BoundServiceManager manager, Logger logger) : base(manager, logger)
        {
            this.mInventory = item;
            this.mFlag = flag;
            this.ItemDB = itemDB;
        }

        public PyDataType List(PyDictionary namedPayload, Client client)
        {
            // get list of all the items with the given flag
            IEnumerable<KeyValuePair<int, ItemEntity>> enumerable;

            if (this.mFlag == ItemFlags.None)
                enumerable = this.mInventory.Items;
            else
                enumerable = this.mInventory.Items
                    .Where(x => x.Value.Flag == this.mFlag);
            
            CRowset result = new CRowset(ItemEntity.sEntityItemDescriptor);

            foreach (KeyValuePair<int, ItemEntity> pair in enumerable)
                result.Add(pair.Value.GetEntityRow());

            return result;
        }

        public PyDataType ListStations(PyInteger blueprintsOnly, PyInteger forCorp, PyDictionary namedPayload,
            Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            // TODO: take into account blueprintsOnly
            if (forCorp == 1)
                return this.ItemDB.ListStations(client.CorporationID);
            else
                return this.ItemDB.ListStations((int) client.CharacterID);
        }

        public PyDataType ListStationItems(PyInteger stationID, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            return this.ItemDB.ListStationItems(stationID, (int) client.CharacterID);
        }
        
        public PyDataType GetItem(PyDictionary namedPayload, Client client)
        {
            return this.mInventory.GetEntityRow();
        }

        public static PyDataType BindInventory(ItemDB itemDB, ItemInventory item, ItemFlags flag, BoundServiceManager boundServiceManager)
        {
            BoundService instance = new BoundInventory(itemDB, item, flag, boundServiceManager, boundServiceManager.Logger);
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