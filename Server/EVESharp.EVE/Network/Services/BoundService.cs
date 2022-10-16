using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Network.Services;

public abstract class BoundService : Service
{
    /// <summary>
    /// The bound service's ID that identifies it
    /// </summary>
    public int BoundID { get; init; }
    /// <summary>
    /// The string version of the bound service's ID that identifies it and it's location
    /// </summary>
    public string BoundString { get; init; }
    /// <summary>
    /// Specific information about the bound service
    /// </summary>
    public PyTuple BoundServiceInformation { get; protected set; }
    /// <summary>
    /// The objectID to which this service was bound to
    /// </summary>
    public int ObjectID { get; init; }
    public IBoundServiceManager BoundServiceManager { get; }

    /// <summary>
    /// Creates a base bound service to no client to be used as a normal service
    /// </summary>
    /// <param name="manager">The bound service manager used by this service</param>
    public BoundService (IBoundServiceManager manager)
    {
        BoundServiceManager = manager;
    }

    /// <summary>
    /// Creates a bound service to the given objectID
    /// </summary>
    /// <param name="manager">The bound service manager used by this service</param>
    /// <param name="objectID">The object it's bound to</param>
    public BoundService (IBoundServiceManager manager, int objectID) : this (manager)
    {
        ObjectID = objectID;

        BoundID     = manager.BindService (this);
        BoundString = manager.BuildBoundServiceString (BoundID);
    }

    /// <summary>
    /// Called by the EVE Client to know which node is storing the object's information
    /// </summary>
    /// <param name="bindParams">The parameters to resolve the object</param>
    /// <param name="justQuery">Indicates if the client is just querying the thing or if it will bind to it</param>
    /// <param name="call"></param>
    /// <returns>The node where this object is stored</returns>
    [MustBeCharacter]
    public PyInteger MachoResolveObject (ServiceCall call, PyDataType bindParams, PyInteger justQuery)
    {
        return this.MachoResolveObject (call, bindParams);
    }

    /// <summary>
    /// Standard resolver for bound services, should handle most of the required cases
    /// </summary>
    /// <param name="parameters">The parameters used for this resolve call</param>
    /// <param name="call">The caller information</param>
    /// <returns>The node where this object is stored</returns>
    protected abstract long MachoResolveObject (ServiceCall call, ServiceBindParams parameters);

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
    [MustBeCharacter]
    public PyDataType MachoBindObject (ServiceCall call, PyDataType bindParams, PyDataType callInfo)
    {
        return this.MachoBindObject (call, (ServiceBindParams) bindParams, callInfo);
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
    protected abstract PyDataType MachoBindObject (ServiceCall call, ServiceBindParams bindParams, PyDataType callInfo);

    /// <summary>
    /// Checks if the caller has enough permissions to use this bound service
    /// </summary>
    /// <param name="session">The session to check against</param>
    /// <returns></returns>
    public abstract bool IsClientAllowedToCall (Session session);

    /// <summary>
    /// Handles when a client frees this object (like when it's disconnected)
    /// </summary>
    /// <param name="session">Session of the user that free'd us</param>
    public abstract void ClientHasReleasedThisObject (Session session);

    /// <summary>
    /// Applies the given session change to the service's cached sessions (if found)
    /// </summary>
    /// <param name="characterID">The character to update sessions for</param>
    /// <param name="changes">The delta of changes</param>
    public abstract void ApplySessionChange (int characterID, PyDictionary <PyString, PyTuple> changes);

    /// <summary>
    /// Destroys this service, unregisters it and notifies the owner of the reference (player) to let it know that
    /// it's not available anymore
    /// </summary>
    public abstract void DestroyService ();
}