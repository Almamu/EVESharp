using System;
using System.Collections.Generic;
using EVESharp.Common.Configuration;
using EVESharp.Common.Network;

namespace EVESharp.EVE.Network.Transports;

public interface ITransportManager
{
    /// <summary>
    /// The current server transport in use
    /// </summary>
    MachoServerTransport ServerTransport { get; }
    /// <summary>
    /// The unvalidated transports
    /// </summary>
    List <MachoTransport> UnauthenticatedTransports { get; }
    /// <summary>
    /// The registered and validated client transports
    /// </summary>
    Dictionary <int, MachoClientTransport> ClientTransports { get; }
    /// <summary>
    /// The registered and validated node transports
    /// </summary>
    Dictionary <long, MachoNodeTransport> NodeTransports { get; }
    /// <summary>
    /// The registered and validated proxy transports
    /// </summary>
    Dictionary <long, MachoProxyTransport> ProxyTransports { get; }
    /// <summary>
    /// Full list of active transports for this node
    /// </summary>
    List <MachoTransport> TransportList { get; }
    /// <summary>
    /// Event fired when a transport is removed
    /// </summary>
    EventHandler <MachoTransport> OnTransportRemoved { get;     set; }
    /// <summary>
    /// Event fired when a transport is resolved to a client
    /// </summary>
    EventHandler <MachoClientTransport> OnClientResolved { get; set; }
    /// <summary>
    /// Event fired when a transport is resolved to a node
    /// </summary>
    EventHandler <MachoNodeTransport>   OnNodeResolved   { get; set; }
    /// <summary>
    /// Event fired when a transport is resolved to a proxy
    /// </summary>
    EventHandler <MachoProxyTransport>  OnProxyResolved  { get; set; }

    /// <summary>
    /// Creates a new server transport to be used
    /// </summary>
    /// <param name="machoNet"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    MachoServerTransport OpenServerTransport (IMachoNet machoNet, MachoNet configuration);
    /// <summary>
    /// Registers a new, waiting to be authenticated transport
    /// </summary>
    /// <param name="machoNet"></param>
    /// <param name="socket"></param>
    void NewTransport (IMachoNet machoNet, EVEClientSocket socket);
    /// <summary>
    /// Registers the given transport as a client's transport
    /// </summary>
    /// <param name="transport"></param>
    void ResolveClientTransport (MachoUnauthenticatedTransport transport);
    /// <summary>
    /// Registers the given transport as a node's transport
    /// </summary>
    /// <param name="transport"></param>
    void ResolveNodeTransport (MachoUnauthenticatedTransport  transport);
    /// <summary>
    /// Registers the given transport as a proxy's transport
    /// </summary>
    /// <param name="transport"></param>
    void ResolveProxyTransport (MachoUnauthenticatedTransport transport);
    void OnTransportTerminated (MachoTransport                transport);
}