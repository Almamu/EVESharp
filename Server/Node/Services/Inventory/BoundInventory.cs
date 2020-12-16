using System;
using Node.Inventory.Items;
using PythonTypes.Types.Primitives;

namespace Node.Services.Inventory
{
    public class BoundInventory : Service
    {
        private ItemInventory mInventory;
        
        public BoundInventory(ItemInventory item, ServiceManager manager) : base(manager)
        {
            this.mInventory = item;
        }

        public static PyDataType BindInventory(ItemInventory item, ServiceManager manager)
        {
            Service instance = new BoundInventory(item, manager);
            // bind the service
            int boundID = manager.Container.BoundServiceManager.BoundService(instance);
            // build the bound service string
            string boundServiceStr = manager.Container.BoundServiceManager.BuildBoundServiceString(boundID);

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