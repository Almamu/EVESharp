using System;
using System.Collections.Generic;
using Node.Services;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Network
{
    public abstract class MultiClientBoundService : BoundService
    {
        /// <summary>
        /// List of clients that have access to this bound service
        /// </summary>
        protected List<Client> Clients { get; init; }
        /// <summary>
        /// List of services registered by objectID
        /// </summary>
        private readonly Dictionary<int, MultiClientBoundService> mRegisteredServices;
        
        public MultiClientBoundService(BoundServiceManager manager) : base(manager)
        {
            this.mRegisteredServices = new Dictionary<int, MultiClientBoundService>();
        }

        public MultiClientBoundService(BoundServiceManager manager, int objectID) : base(manager, objectID)
        {
            this.Clients = new List<Client>();
        }

        /// <summary>
        /// Binds a new object of this type with the given objectData to provide a stateful
        /// interface to itself
        /// 
        /// WARNING: Some MachoBindObject calls also include a call to a method inside the new stateful
        /// service, this also handles that behaviour
        /// </summary>
        /// <param name="bindParams">The information of the object to be stateful about</param>
        /// <param name="callInfo">The information on the call</param>
        /// <param name="call">The call object with extra information</param>
        /// <returns></returns>
        protected override PyDataType MachoBindObject(ServiceBindParams bindParams, PyDataType callInfo,
            CallInformation call)
        {
            // create the bound instance and register it in the bound services
            BoundService instance = this.CreateBoundInstance(bindParams, call);

            // TODO: the expiration time is 1 day, might be better to properly support this?
            // TODO: investigate these a bit more closely in the future
            // TODO: i'm not so sure about the expiration time
            PyTuple boundServiceInformation = new PyTuple(2)
            {
                [0] = instance.BoundString,
                [1] = DateTime.UtcNow.Add(TimeSpan.FromDays(1)).ToFileTime()
            };

            // after the service is bound the call can be run (if required)
            PyTuple result = new PyTuple(2)
            {
                [0] = new PySubStruct(new PySubStream(boundServiceInformation)),
                [1] = null
            };
            
            if (callInfo is not null)
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
                
                result[1] = this.BoundServiceManager.ServiceCall(instance.BoundID, func, arguments, callInformation);
            }

            return result;
        }

        /// <summary>
        /// Method to be override by BoundServices to build their own stateful versions when requested
        /// </summary>
        /// <param name="bindParams">The information required for the instantiation</param>
        /// <returns>The new boudn service</returns>
        /// <exception cref="NotImplementedException">If this has not been implemented by the class</exception>
        protected abstract BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call);
    }
}