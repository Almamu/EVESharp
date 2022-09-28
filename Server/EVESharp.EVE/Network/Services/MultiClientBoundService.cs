using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EVESharp.EVE.Sessions;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Network.Services;

public abstract class MultiClientBoundService : BoundService
{
    /// <summary>
    /// List of services registered by objectID
    /// </summary>
    private readonly Dictionary <int, MultiClientBoundService> mRegisteredServices;
    /// <summary>
    /// List of clients that have access to this bound service
    /// </summary>
    protected ConcurrentDictionary <int, Session> Sessions { get; } = new ConcurrentDictionary <int, Session> ();
    /// <summary>
    /// The bound service that created this entity
    /// </summary>
    protected MultiClientBoundService Parent { get; init; }

    /// <summary>
    /// Indicates whether the service has to be kept alive or not
    /// </summary>
    protected bool KeepAlive { get; init; }

    protected MultiClientBoundService (IBoundServiceManager manager, bool keepAlive = false) : base (manager)
    {
        this.mRegisteredServices = new Dictionary <int, MultiClientBoundService> ();
        KeepAlive                = keepAlive;
    }

    protected MultiClientBoundService (MultiClientBoundService parent, int objectID, bool keepAlive = false) : base (parent.BoundServiceManager, objectID)
    {
        Parent    = parent;
        KeepAlive = keepAlive;
    }

    protected MultiClientBoundService (IBoundServiceManager manager, int objectID, bool keepAlive = false) : base (manager, objectID)
    {
        KeepAlive = keepAlive;
    }

    /// <summary>
    /// Finds a bound service based on it's object ID
    /// </summary>
    /// <param name="objectID"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    public bool FindInstanceForObjectID <T> (int objectID, out T service) where T : MultiClientBoundService
    {
        lock (this.mRegisteredServices)
        {
            service = null;

            if (this.mRegisteredServices.TryGetValue (objectID, out MultiClientBoundService tmp) == false)
                return false;

            service = tmp as T;

            return true;
        }
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
    protected override PyDataType MachoBindObject (ServiceCall call, ServiceBindParams bindParams, PyDataType callInfo)
    {
        MultiClientBoundService instance;

        lock (this.mRegisteredServices)
        {
            // check if this object is already registered in our list
            if (this.mRegisteredServices.TryGetValue (bindParams.ObjectID, out instance) == false)
            {
                // TODO: ensure that the request is at the right node and throw an util.UpdateMoniker if it's not right
                // create the bound instance and register it in the bound services
                instance = this.CreateBoundInstance (call, bindParams);
                // store the new service in the list of services
                this.mRegisteredServices [instance.ObjectID] = instance;
            }
        }

        // add the client to the list
        if (instance.Sessions.TryAdd (call.Session.EnsureCharacterIsSelected (), call.Session) == false)
            throw new Exception ("Cannot register the bound service to the character");

        // TODO: the expiration time is 1 day, might be better to properly support this?
        // TODO: investigate these a bit more closely in the future
        // TODO: i'm not so sure about the expiration time
        BoundServiceInformation = new PyTuple (2)
        {
            [0] = instance.BoundString,
            [1] = Guid.NewGuid ().ToString () // ReferenceID, this should be unique
        };

        // after the service is bound the call can be run (if required)
        PyTuple result = new PyTuple (2)
        {
            [0] = new PySubStruct (new PySubStream (BoundServiceInformation)),
            [1] = null
        };
        
        // ensure the session is properly registered in the session manager
        BoundServiceManager.MachoNet.SessionManager.RegisterSession (call.Session);

        if (callInfo is not null)
        {
            PyTuple      data           = callInfo as PyTuple;
            string       func           = data [0] as PyString;
            PyTuple      arguments      = data [1] as PyTuple;
            PyDictionary namedArguments = data [2] as PyDictionary;

            ServiceCall callInformation = new ServiceCall
            {
                Session             = call.Session,
                Payload             = arguments,
                NamedPayload        = namedArguments,
                CallID              = call.CallID,
                Source              = call.Source,
                Destination         = call.Destination,
                ServiceManager      = call.ServiceManager,
                BoundServiceManager = call.BoundServiceManager
            };

            result [1] = BoundServiceManager.ServiceCall (instance.BoundID, func, callInformation);
        }

        // signal that the object was bound, this will be used by the proxy to notify this node on important stuff
        call.ResultOutOfBounds ["OID+"] = new PyList <PyInteger> {BoundServiceInformation};

        return result;
    }

    /// <summary>
    /// Method to be override by BoundServices to build their own stateful versions when requested
    /// </summary>
    /// <param name="bindParams">The information required for the instantiation</param>
    /// <returns>The new boudn service</returns>
    /// <exception cref="NotImplementedException">If this has not been implemented by the class</exception>
    protected abstract MultiClientBoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams);

    protected virtual void OnClientDisconnected (Session session) { }

    /// <summary>
    /// Handles when a client frees this object (like when it's disconnected)
    /// </summary>
    /// <param name="session">Session of the user that free'd us</param>
    public override void ClientHasReleasedThisObject (Session session)
    {
        // call any freeing code (if any)
        this.OnClientDisconnected (session);
        // remove the client from the list
        Sessions.Remove (session.EnsureCharacterIsSelected (), out _);

        if (Sessions.Count == 0 && KeepAlive == false)
        {
            // tell the bound service manager we're dying
            BoundServiceManager.UnbindService (this);
            // check for the parent service and unregister ourselves from it
            Parent?.mRegisteredServices.Remove (ObjectID);
        }
    }

    /// <summary>
    /// Applies the given session change to the service's cached sessions (if found)
    /// </summary>
    /// <param name="characterID">The character to update the session for</param>
    /// <param name="changes">The delta of changes</param>
    public override void ApplySessionChange (int characterID, PyDictionary <PyString, PyTuple> changes)
    {
        if (Sessions.TryGetValue (characterID, out Session session) == false)
            return;

        session.ApplyDelta (changes);
    }
}