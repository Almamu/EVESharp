using System;
using System.Collections.Generic;

namespace EVESharp.EVE.Network.Transports;

public interface ITransportManager
{
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
    EventHandler <MachoClientTransport> OnClientResolved { get; set; }
    EventHandler <MachoNodeTransport>   OnNodeResolved   { get; set; }
    EventHandler <MachoProxyTransport>  OnProxyResolved  { get; set; }
    void                NewTransport (MachoUnauthenticatedTransport transport);

    /// <summary>
    /// Registers the given transport as a client's transport
    /// </summary>
    /// <param name="transport"></param>
    void ResolveClientTransport (MachoUnauthenticatedTransport transport);

    void ResolveNodeTransport (MachoUnauthenticatedTransport  transport);
    void ResolveProxyTransport (MachoUnauthenticatedTransport transport);
    void OnTransportTerminated (MachoTransport                transport);
}