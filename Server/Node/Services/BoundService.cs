using System;
using Common.Services;
using Node.Network;
using PythonTypes.Types.Primitives;

namespace Node.Services
{
    public abstract class BoundService : Service
    {
        public BoundServiceManager BoundServiceManager { get; }
        /// <summary>
        /// The client that owns this bound service
        /// </summary>
        public Client Client { get; }
        public BoundService(BoundServiceManager manager, Client client)
        {
            this.BoundServiceManager = manager;
            this.Client = client;
        }

        /// <summary>
        /// Called by the EVE Client to know which node is storing the objectID's information
        /// TODO: PROPERLY SUPPORT THE ENTITY TABLE NODEID FIELD TO GET THIS INFORMATION PROPERLY
        /// TODO: OR LET THE CHILD CLASS HANDLE THIS
        ///
        /// TODO: FOR NOW JUST RETURN OUR NODE ID AND BE HAPPY ABOUT IT
        /// </summary>
        /// <param name="objectData"></param>
        /// <param name="zero"></param>
        /// <param name="call"></param>
        /// <returns>The node where this object is stored</returns>
        public virtual PyInteger MachoResolveObject(PyTuple objectData, PyInteger zero, CallInformation call)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called by the EVE Client to know which node is storing the objectID's information
        /// TODO: PROPERLY SUPPORT THE ENTITY TABLE NODEID FIELD TO GET THIS INFORMATION PROPERLY
        /// TODO: OR LET THE CHILD CLASS HANDLE THIS
        ///
        /// TODO: FOR NOW JUST RETURN OUR NODE ID AND BE HAPPY ABOUT IT
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="zero"></param>
        /// <param name="call"></param>
        /// <returns>The node where this object is stored</returns>
        public virtual PyInteger MachoResolveObject(PyInteger stationID, PyInteger zero, CallInformation call)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Binds a new object of this type with the given objectData to provide a stateful
        /// interface to itself
        ///
        /// WARNING: Some MachoBindObject calls also include a call to a method inside the new stateful
        /// service, this also handles that behaviour
        /// </summary>
        /// <param name="objectData">The information of the object to be stateful about</param>
        /// <param name="callInfo">The information on the call</param>
        /// <param name="call">The call object with extra information</param>
        /// <returns></returns>
        public PyDataType MachoBindObject(PyTuple objectData, PyTuple callInfo, CallInformation call)
        {
            return this.MachoBindObject(objectData, callInfo as PyDataType, call);
        }
        
        /// <summary>
        /// Binds a new object of this type with the given objectData to provide a stateful
        /// interface to itself
        ///
        /// WARNING: Some MachoBindObject calls also include a call to a method inside the new stateful
        /// service, this also handles that behaviour
        /// </summary>
        /// <param name="objectData">The information of the object to be stateful about</param>
        /// <param name="callInfo">The information on the call</param>
        /// <param name="call">The call object with extra information</param>
        /// <returns></returns>
        public PyDataType MachoBindObject(PyTuple objectData, PyNone callInfo, CallInformation call)
        {
            return this.MachoBindObject(objectData, callInfo as PyDataType, call);
        }
        
        /// <summary>
        /// Binds a new object of this type with the given objectData to provide a stateful
        /// interface to itself
        ///
        /// WARNING: Some MachoBindObject calls also include a call to a method inside the new stateful
        /// service, this also handles that behaviour
        /// </summary>
        /// <param name="objectID">The information of the object to be stateful about</param>
        /// <param name="callInfo">The information on the call</param>
        /// <param name="call">The call object with extra information</param>
        /// <returns></returns>
        public PyDataType MachoBindObject(PyInteger objectID, PyNone callInfo, CallInformation call)
        {
            return this.MachoBindObject(objectID, callInfo as PyDataType, call);
        }
        
        /// <summary>
        /// Binds a new object of this type with the given objectData to provide a stateful
        /// interface to itself
        ///
        /// WARNING: Some MachoBindObject calls also include a call to a method inside the new stateful
        /// service, this also handles that behaviour
        /// </summary>
        /// <param name="objectID">The information of the object to be stateful about</param>
        /// <param name="callInfo">The information on the call</param>
        /// <param name="call">The call object with extra information</param>
        /// <returns></returns>
        public PyDataType MachoBindObject(PyInteger objectID, PyTuple callInfo, CallInformation call)
        {
            return this.MachoBindObject(objectID, callInfo as PyDataType, call);
        }
        
        /// <summary>
        /// Binds a new object of this type with the given objectData to provide a stateful
        /// interface to itself
        ///
        /// WARNING: Some MachoBindObject calls also include a call to a method inside the new stateful
        /// service, this also handles that behaviour
        /// </summary>
        /// <param name="objectData">The information of the object to be stateful about</param>
        /// <param name="callInfo">The information on the call</param>
        /// <param name="call">The call object with extra information</param>
        /// <returns></returns>
        protected PyDataType MachoBindObject(PyDataType objectData, PyDataType callInfo, CallInformation call)
        {
            // create the bound instance and register it in the bound services
            BoundService instance = this.CreateBoundInstance(objectData, call);

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
                string func = data[0] as PyString;
                PyTuple arguments = data[1] as PyTuple;
                PyDictionary namedArguments = data[2] as PyDictionary;

                CallInformation callInformation = new CallInformation
                {
                    Client = call.Client,
                    NamedPayload = namedArguments,
                    CallID = call.CallID,
                    From = call.From,
                    PacketType = call.PacketType,
                    Service = null,
                    To = call.To
                };
                
                result[1] = this.BoundServiceManager.ServiceCall(boundID, func, arguments, callInformation);
            }

            return result;
        }

        /// <summary>
        /// Method to be override by BoundServices to build their own stateful versions when requested
        /// </summary>
        /// <param name="objectData">The information required for the instantiation</param>
        /// <returns>The new boudn service</returns>
        /// <exception cref="NotImplementedException">If this has not been implemented by the class</exception>
        protected virtual BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method called when this service is free'd to perform any required cleanup
        /// </summary>
        public virtual void OnServiceFree()
        {
            
        }
    }
}