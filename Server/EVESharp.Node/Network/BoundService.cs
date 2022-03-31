using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Services;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
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
        public BoundServiceManager BoundServiceManager { get; }
        
        /// <summary>
        /// Creates a base bound service to no client to be used as a normal service
        /// </summary>
        /// <param name="manager">The bound service manager used by this service</param>
        public BoundService(BoundServiceManager manager)
        {
            this.BoundServiceManager = manager;
        }

        /// <summary>
        /// Creates a bound service to the given objectID
        /// </summary>
        /// <param name="manager">The bound service manager used by this service</param>
        /// <param name="objectID">The object it's bound to</param>
        public BoundService(BoundServiceManager manager, int objectID) : this(manager)
        {
            this.ObjectID = objectID;
        
            this.BoundID = manager.BindService(this);
            this.BoundString = manager.BuildBoundServiceString(this.BoundID);
        }

        /// <summary>
        /// Called by the EVE Client to know which node is storing the object's information
        /// </summary>
        /// <param name="bindParams">The parameters to resolve the object</param>
        /// <param name="zero">Allways zero?</param>
        /// <param name="call"></param>
        /// <returns>The node where this object is stored</returns>
        public PyInteger MachoResolveObject(PyDataType bindParams, PyInteger zero, CallInformation call)
        {
            return this.MachoResolveObject(bindParams, call);
        }

        /// <summary>
        /// Standard resolver for bound services, should handle most of the required cases
        /// </summary>
        /// <param name="parameters">The parameters used for this resolve call</param>
        /// <param name="call">The caller information</param>
        /// <returns>The node where this object is stored</returns>
        protected abstract long MachoResolveObject(ServiceBindParams parameters, CallInformation call);
        
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
        public PyDataType MachoBindObject(PyDataType bindParams, PyDataType callInfo, CallInformation call)
        {
            return this.MachoBindObject((ServiceBindParams) bindParams, callInfo, call);
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
        protected abstract PyDataType MachoBindObject(ServiceBindParams bindParams, PyDataType callInfo, CallInformation call);

        /// <summary>
        /// Checks if the caller has enough permissions to use this bound service
        /// </summary>
        /// <param name="session">The session to check against</param>
        /// <returns></returns>
        public abstract bool IsClientAllowedToCall(Session session);

        /// <summary>
        /// Handles when a client frees this object (like when it's disconnected)
        /// </summary>
        /// <param name="session">Session of the user that free'd us</param>
        public abstract void ClientHasReleasedThisObject(Session session);

        /// <summary>
        /// Applies the given session change to the service's cached sessions (if found)
        /// </summary>
        /// <param name="userID">The user to update sessions for</param>
        /// <param name="changes">The delta of changes</param>
        public abstract void ApplySessionChange(long userID, PyDictionary<PyString, PyTuple> changes);
    }
}