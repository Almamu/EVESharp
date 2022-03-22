using System;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    public abstract class ClientBoundService : BoundService
    {
        /// <summary>
        /// The client that owns this bound service
        /// </summary>
        public Session Session { get; }
        
        /// <summary>
        /// Creates a base bound service to no client to be used as a normal service
        /// </summary>
        /// <param name="manager">The bound service manager used by this service</param>
        public ClientBoundService(BoundServiceManager manager) : base(manager)
        {
        }

        /// <summary>
        /// Creates a bound service to the given objectID
        /// </summary>
        /// <param name="manager">The bound service manager used by this service</param>
        /// <param name="client">The client that it belongs to</param>
        /// <param name="objectID">The object it's bound to</param>
        public ClientBoundService(BoundServiceManager manager, Session session, int objectID) : base(manager, objectID)
        {
            this.Session = session;
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
        protected override PyDataType MachoBindObject(ServiceBindParams bindParams, PyDataType callInfo, CallInformation call)
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
                    MachoNet = call.MachoNet,
                    CallID = call.CallID,
                    Destination = call.Destination,
                    Source = call.Source,
                    Payload = arguments,
                    NamedPayload = namedArguments,
                    Session = call.Session
                };
                
                result[1] = this.BoundServiceManager.ServiceCall(instance.BoundID, func, callInformation);
            }
            
            // signal that the object was bound, this will be used by the proxy to notify this node on important stuff
            call.ResutOutOfBounds["OID+"] = new PyList<PyInteger>() {instance.BoundID};

            return result;
        }

        /// <summary>
        /// Method to be override by BoundServices to build their own stateful versions when requested
        /// </summary>
        /// <param name="bindParams">The information required for the instantiation</param>
        /// <returns>The new boudn service</returns>
        /// <exception cref="NotImplementedException">If this has not been implemented by the class</exception>
        protected abstract BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call);

        protected virtual void OnClientDisconnected()
        {
            
        }

        public override bool IsClientAllowedToCall(Session session)
        {
            return this.Session.UserID == session.UserID;
        }

        public override void ClientHasReleasedThisObject(Session session)
        {
            // first call any freeing code (if any)
            this.OnClientDisconnected();
            // then tell the bound service that we are not alive anymore
            this.BoundServiceManager.UnbindService(this);
        }

        /// <summary>
        /// Applies the given session change to the service's cached sessions (if found)
        /// </summary>
        /// <param name="userID">The user to update sessions for</param>
        /// <param name="changes">The delta of changes</param>
        public override void ApplySessionChange(long userID, PyDictionary<PyString, PyTuple> changes)
        {
            if (this.Session.UserID != userID)
                return;
            
            this.Session.ApplyDelta(changes);
        }
    }
}