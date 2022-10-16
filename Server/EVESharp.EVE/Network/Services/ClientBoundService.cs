using System;
using EVESharp.Common.Configuration;
using EVESharp.EVE.Notifications.Network;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types.Network;
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

    public override void DestroyService ()
    {
        // TODO: PROPERLY IMPLEMENT AND CLEAN THIS UP LATER
        PyTuple data = new OnMachoObjectDisconnect (this.BoundString, Session.UserID, BoundServiceInformation [1] as PyString);
        
        PyTuple dataContainer = new PyTuple (2)
        {
            [0] = 1, // gpcs.ObjectCall::ObjectCall
            [1] = data
        };

        dataContainer = new PyTuple (2)
        {
            [0] = 0, // gpcs.ServiceCall::NotifyDown
            [1] = dataContainer
        };

        dataContainer = new PyTuple (2)
        {
            [0] = 0, // gpcs.ObjectCall::NotifyDown
            [1] = new PySubStream (dataContainer)
        };

        dataContainer = new PyTuple (2)
        {
            [0] = dataContainer,
            [1] = null
        };

        string idType        = Session.USERID;
        PyList idsOfInterest = new PyList () {Session.UserID};

        PyPacket packet = new PyPacket (PyPacket.PacketType.NOTIFICATION)
        {
            Destination = new PyAddressBroadcast (idsOfInterest, idType, "OnMachoObjectDisconnect"),
            Source      = new PyAddressNode (BoundServiceManager.MachoNet.NodeID),

            // set the userID to -1, this will indicate the cluster controller to fill it in
            UserID  = -1,
            Payload = dataContainer,
            OutOfBounds = new PyDictionary()
            {
                {"OID-", BoundServiceInformation [1]}
            }
        };

        BoundServiceManager.MachoNet.QueueOutputPacket (packet);
        
        // remove ourselves from the list
        BoundServiceManager.UnbindService (this);
    }
}