﻿using System;
using System.Collections.Generic;
using EVESharp.EVE;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Services;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    public abstract class MultiClientBoundService : BoundService
    {
        /// <summary>
        /// List of clients that have access to this bound service
        /// </summary>
        protected Dictionary<long, Session> Sessions { get; } = new Dictionary<long, Session>();
        /// <summary>
        /// The bound service that created this entity
        /// </summary>
        protected MultiClientBoundService Parent { get; init; }

        /// <summary>
        /// Indicates whether the service has to be kept alive or not
        /// </summary>
        protected bool KeepAlive { get; init; } = false;
        
        /// <summary>
        /// List of services registered by objectID
        /// </summary>
        private readonly Dictionary<int, MultiClientBoundService> mRegisteredServices;
        
        public MultiClientBoundService(BoundServiceManager manager, bool keepAlive = false) : base(manager)
        {
            this.mRegisteredServices = new Dictionary<int, MultiClientBoundService>();
            this.KeepAlive = keepAlive;
        }

        public MultiClientBoundService(MultiClientBoundService parent, int objectID, bool keepAlive = false) : base(parent.BoundServiceManager, objectID)
        {
            this.Parent = parent;
            this.KeepAlive = keepAlive;
        }

        public MultiClientBoundService(BoundServiceManager manager, int objectID, bool keepAlive = false) : base(manager, objectID)
        {
            this.KeepAlive = keepAlive;
        }

        /// <summary>
        /// Finds a bound service based on it's object ID
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public bool FindInstanceForObjectID<T>(int objectID, out T service) where T : MultiClientBoundService
        {
            service = null;
            
            if (this.mRegisteredServices.TryGetValue(objectID, out MultiClientBoundService tmp) == false)
                return false;

            service = tmp as T;
            
            return true;
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
            MultiClientBoundService instance;
            
            lock (this.mRegisteredServices)
            {
                // check if this object is already registered in our list
                if (this.mRegisteredServices.TryGetValue(bindParams.ObjectID, out instance) == false)
                {
                    // create the bound instance and register it in the bound services
                    instance = this.CreateBoundInstance(bindParams, call);
                    // store the new service in the list of services
                    this.mRegisteredServices[instance.ObjectID] = instance;
                }
            }

            // add the client to the list
            instance.Sessions.Add(call.Session.EnsureCharacterIsSelected(), call.Session);
            
            // TODO: the expiration time is 1 day, might be better to properly support this?
            // TODO: investigate these a bit more closely in the future
            // TODO: i'm not so sure about the expiration time
            this.BoundServiceInformation = new PyTuple(2)
            {
                [0] = instance.BoundString,
                [1] = DateTime.UtcNow.Add(TimeSpan.FromDays(1)).ToFileTime()
            };

            // after the service is bound the call can be run (if required)
            PyTuple result = new PyTuple(2)
            {
                [0] = new PySubStruct(new PySubStream(this.BoundServiceInformation)),
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
                    Session = call.Session,
                    Payload = arguments,
                    NamedPayload = namedArguments,
                    CallID = call.CallID,
                    Source = call.Source,
                    Destination = call.Destination,
                    ServiceManager = call.ServiceManager,
                    BoundServiceManager = call.BoundServiceManager
                };
                
                result[1] = this.BoundServiceManager.ServiceCall(instance.BoundID, func, callInformation);
            }
            
            // signal that the object was bound, this will be used by the proxy to notify this node on important stuff
            call.ResultOutOfBounds["OID+"] = new PyList<PyInteger>() {this.BoundServiceInformation};

            return result;
        }

        /// <summary>
        /// Method to be override by BoundServices to build their own stateful versions when requested
        /// </summary>
        /// <param name="bindParams">The information required for the instantiation</param>
        /// <returns>The new boudn service</returns>
        /// <exception cref="NotImplementedException">If this has not been implemented by the class</exception>
        protected abstract MultiClientBoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call);

        protected virtual void OnClientDisconnected(Session session)
        {
            
        }

        /// <summary>
        /// Handles when a client frees this object (like when it's disconnected)
        /// </summary>
        /// <param name="session">Session of the user that free'd us</param>
        public override void ClientHasReleasedThisObject(Session session)
        {
            // call any freeing code (if any)
            this.OnClientDisconnected(session);
            // remove the client from the list
            this.Sessions.Remove(session.UserID);

            if (this.Sessions.Count == 0 && this.KeepAlive == false)
            {
                // tell the bound service manager we're dying
                this.BoundServiceManager.UnbindService(this);
                // check for the parent service and unregister ourselves from it
                this.Parent?.mRegisteredServices.Remove(this.ObjectID);
            }
        }

        /// <summary>
        /// Applies the given session change to the service's cached sessions (if found)
        /// </summary>
        /// <param name="userID">The user to update sessions for</param>
        /// <param name="changes">The delta of changes</param>
        public override void ApplySessionChange(long userID, PyDictionary<PyString, PyTuple> changes)
        {
            if (this.Sessions.TryGetValue(userID, out Session session) == false)
                return;

            session.ApplyDelta(changes);
        }
    }
}