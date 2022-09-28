using System;
using EVESharp.EVE.Sessions;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Network.Services;

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
    protected ClientBoundService (IBoundServiceManager manager) : base (manager) { }

    /// <summary>
    /// Creates a bound service to the given objectID
    /// </summary>
    /// <param name="manager">The bound service manager used by this service</param>
    /// <param name="session">The client that it belongs to</param>
    /// <param name="objectID">The object it's bound to</param>
    protected ClientBoundService (IBoundServiceManager manager, Session session, int objectID) : base (manager, objectID)
    {
        Session = session;
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
        // TODO: ensure that the request is at the right node and throw an util.UpdateMoniker if it's not right
        // create the bound instance and register it in the bound services
        BoundService instance = this.CreateBoundInstance (call, bindParams);

        // TODO: the expiration time is 1 day, might be better to properly support this?
        // TODO: investigate these a bit more closely in the future
        // TODO: i'm not so sure about the expiration time
        BoundServiceInformation = new PyTuple (2)
        {
            [0] = instance.BoundString,
            [1] = Guid.NewGuid ().ToString() // ReferenceID, this should be unique
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
                MachoNet            = call.MachoNet,
                CallID              = call.CallID,
                Destination         = call.Destination,
                Source              = call.Source,
                Payload             = arguments,
                NamedPayload        = namedArguments,
                Session             = call.Session,
                BoundServiceManager = call.BoundServiceManager,
                ServiceManager      = call.ServiceManager
            };

            result [1] = BoundServiceManager.ServiceCall (instance.BoundID, func, callInformation);
        }

        // signal that the object was bound, this will be used by the proxy to notify this node on important stuff
        call.ResultOutOfBounds ["OID+"] = new PyList <PyTuple> {BoundServiceInformation};

        return result;
    }

    /// <summary>
    /// Method to be override by BoundServices to build their own stateful versions when requested
    /// </summary>
    /// <param name="bindParams">The information required for the instantiation</param>
    /// <returns>The new boudn service</returns>
    /// <exception cref="NotImplementedException">If this has not been implemented by the class</exception>
    protected abstract BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams);

    protected virtual void OnClientDisconnected () { }

    public override bool IsClientAllowedToCall (Session session)
    {
        return Session.UserID == session.UserID;
    }

    public override void ClientHasReleasedThisObject (Session session)
    {
        // ensure the session is the same as the one that created the service
        if (IsClientAllowedToCall (session) == false)
            return;
        
        // first call any freeing code (if any)
        this.OnClientDisconnected ();
        // then tell the bound service that we are not alive anymore
        BoundServiceManager.UnbindService (this);
    }

    /// <summary>
    /// Applies the given session change to the service's cached sessions (if found)
    /// </summary>
    /// <param name="characterID">The characterID to update the session for</param>
    /// <param name="changes">The delta of changes</param>
    public override void ApplySessionChange (int characterID, PyDictionary <PyString, PyTuple> changes)
    {
        if (Session.CharacterID != characterID)
            return;

        Session.ApplyDelta (changes);
    }
}