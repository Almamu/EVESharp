using System;
using System.Threading.Tasks;
using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Packets;

namespace EVESharp.EVE.Network;

public interface IClusterManager
{
    /// <summary>
    /// The event used when a timed event has to happen cluster-wide
    /// </summary>
    event EventHandler ClusterTimerTick;

    /// <summary>
    /// Register the given IMachoNet instance with the orchestrator and updates it's information
    /// </summary>
    void RegisterNode ();

    /// <summary>
    /// Sends a heartbeat to the orchestrator agent to signal our node being up and running healthily
    /// </summary>
    void PerformHeartbeat ();

    /// <summary>
    /// Ensures the identification req is actually legitimate
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    Task <bool> ValidateIdentificationReq (IdentificationReq req);

    /// <summary>
    /// Opens a connection to the given proxy
    /// </summary>
    /// <param name="nodeID">The nodeID of the proxy to connect to</param>
    Task <IMachoTransport> OpenNodeConnection (long nodeID);

    void EstablishConnectionWithProxies ();

    /// <summary>
    /// Contacts the orchestrator and gets the node with less load
    /// </summary>
    /// <returns></returns>
    Task<long> GetLessLoadedNode ();
}