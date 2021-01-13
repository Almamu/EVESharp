using System;
using Common.Logging;
using Common.Services;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services
{
    public abstract class BoundService : Service
    {
        protected readonly Channel Log;
        public BoundServiceManager BoundServiceManager { get; }
        public BoundService(BoundServiceManager manager, Logger logger)
        {
            this.Log = logger.CreateLogChannel("BoundService");
            this.BoundServiceManager = manager;
        }

        public PyDataType MachoResolveObject(PyTuple objectData, PyInteger zero, PyDictionary namedPayload,
            Client client)
        {
            PyInteger objectID = objectData[0] as PyInteger;
            
            // TODO: PROPERLY SUPPORT THE ENTITY TABLE NODEID FIELD TO GET THIS INFORMATION PROPERLY
            // TODO: FOR NOW JUST RETURN OUR ID AND BE HAPPY ABOUT IT
            
            return this.BoundServiceManager.Container.NodeID;
        }

        public PyDataType MachoResolveObject(PyInteger objectID, PyInteger zero, PyDictionary namedPayload,
            Client client)
        {
            // TODO: PROPERLY SUPPORT THE ENTITY TABLE NODEID FIELD TO GET THIS INFORMATION PROPERLY
            // TODO: FOR NOW JUST RETURN OUR ID AND BE HAPPY ABOUT IT
            return this.BoundServiceManager.Container.NodeID;
        }

        public PyDataType MachoBindObject(PyTuple objectData, PyTuple callInfo, PyDictionary namedPayload,
            Client client)
        {
            return this.MachoBindObject(objectData, callInfo as PyDataType, namedPayload, client);
        }

        public PyDataType MachoBindObject(PyTuple objectData, PyNone callInfo, PyDictionary namedPayload,
            Client client)
        {
            return this.MachoBindObject(objectData, callInfo as PyDataType, namedPayload, client);
        }

        public PyDataType MachoBindObject(PyInteger objectID, PyNone callInfo, PyDictionary namedPayload, Client client)
        {
            return this.MachoBindObject(objectID, callInfo as PyDataType, namedPayload, client);
        }
        
        protected PyDataType MachoBindObject(PyDataType objectData, PyDataType callInfo, PyDictionary namedPayload,
            Client client)
        {
            // create the bound instance and register it in the bound services
            BoundService instance = this.CreateBoundInstance(objectData);

            // bind the service
            int boundID = this.BoundServiceManager.BoundService(instance);
            // build the bound service string
            string boundServiceStr = this.BoundServiceManager.BuildBoundServiceString(boundID);

            // TODO: the expiration time is 1 day, might be better to properly support this?
            // TODO: investigate these a bit more closely in the future
            // TODO: i'm not so sure about the expiration time
            PyTuple boundServiceInformation = new PyTuple(new PyDataType[]
            {
                boundServiceStr, DateTime.UtcNow.Add(TimeSpan.FromDays(1)).ToFileTime()
            });

            // after the service is bound the call can be run (if required)
            PyTuple result = new PyTuple(2);

            result[0] = new PySubStruct(new PySubStream(boundServiceInformation));

            if (callInfo is PyNone)
                result[1] = null;
            else
            {
                PyTuple data = callInfo as PyTuple;
                string call = data[0] as PyString;
                PyTuple arguments = data[1] as PyTuple;
                PyDictionary namedArguments = data[2] as PyDictionary;
                
                Log.Trace($"Calling {GetType().Name}::{call} on bound service {boundID}");
                
                result[1] = this.BoundServiceManager.ServiceCall(
                    boundID, call, arguments, namedArguments, client
                );
            }

            return result;
        }

        protected virtual BoundService CreateBoundInstance(PyDataType objectData)
        {
            throw new NotImplementedException();
        }
    }
}