using System;
using System.Collections.Generic;
using System.Net.Http;
using EVESharp.Common.Configuration;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Transports;
using EVESharp.Node.Configuration;
using Serilog;

namespace EVESharp.Node.Server.Shared.Transports;

public class TransportManager : ITransportManager
{
    /// <summary>
    /// The current server transport in use
    /// </summary>
    public MachoServerTransport ServerTransport { get; private set; }
    /// <summary>
    /// The unvalidated transports
    /// </summary>
    public List <MachoTransport> UnauthenticatedTransports { get; } = new List <MachoTransport> ();
    /// <summary>
    /// The registered and validated client transports
    /// </summary>
    public Dictionary <int, MachoClientTransport> ClientTransports { get; } = new Dictionary <int, MachoClientTransport> ();
    /// <summary>
    /// The registered and validated node transports
    /// </summary>
    public Dictionary <long, MachoNodeTransport> NodeTransports { get; } = new Dictionary <long, MachoNodeTransport> ();
    /// <summary>
    /// The registered and validated proxy transports
    /// </summary>
    public Dictionary <long, MachoProxyTransport> ProxyTransports { get; } = new Dictionary <long, MachoProxyTransport> ();
    /// <summary>
    /// Full list of active transports for this node
    /// </summary>
    public List <MachoTransport> TransportList { get; } = new List <MachoTransport> ();
    protected ILogger    Log        { get; }
    protected HttpClient HttpClient { get; }

    /// <summary>
    /// Event fired when a transport is removed
    /// </summary>
    public EventHandler <MachoTransport> OnTransportRemoved { get;     set; }
    public EventHandler <MachoClientTransport> OnClientResolved { get; set; }
    public EventHandler <MachoNodeTransport>   OnNodeResolved   { get; set; }
    public EventHandler <MachoProxyTransport>  OnProxyResolved  { get; set; }

    public TransportManager (HttpClient httpClient, ILogger logger)
    {
        Log        = logger;
        HttpClient = httpClient;
    }

    public MachoServerTransport OpenServerTransport (IMachoNet machoNet, MachoNet configuration)
    {
        return this.ServerTransport = new MachoServerTransport (configuration.Port, machoNet, Log.ForContext <MachoServerTransport> ("Listener"));
    }

    public void NewTransport (IMachoNet machoNet, EVEClientSocket socket)
    {
        UnauthenticatedTransports.Add (
            new MachoUnauthenticatedTransport (
                machoNet, this.HttpClient, socket, Log.ForContext <MachoUnauthenticatedTransport> (socket.GetRemoteAddress ())
            )
        );
    }

    /// <summary>
    /// Registers the given transport as a client's transport
    /// </summary>
    /// <param name="transport"></param>
    public void ResolveClientTransport (MachoUnauthenticatedTransport transport)
    {
        // first remove the transport from the unauthenticated list
        UnauthenticatedTransports.Remove (transport);

        // create the new client transport and store it somewhere
        MachoClientTransport newTransport = new MachoClientTransport (transport);

        if (ClientTransports.TryGetValue (newTransport.Session.UserID, out MachoClientTransport original))
            original.AbortConnection ();

        ClientTransports.Add (newTransport.Session.UserID, newTransport);
        TransportList.Add (newTransport);

        OnClientResolved?.Invoke (this, newTransport);
    }

    public void ResolveNodeTransport (MachoUnauthenticatedTransport transport)
    {
        Log.Information ($"Connection from server with nodeID {transport.Session.NodeID}");
        // first remove the transport from the unauthenticated list
        UnauthenticatedTransports.Remove (transport);

        // create the new client transport and store it somewhere
        MachoNodeTransport newTransport = new MachoNodeTransport (transport);

        if (NodeTransports.TryGetValue (newTransport.Session.NodeID, out MachoNodeTransport original))
            original.AbortConnection ();

        NodeTransports.Add (newTransport.Session.NodeID, newTransport);
        TransportList.Add (newTransport);

        OnNodeResolved?.Invoke (this, newTransport);
    }

    public void ResolveProxyTransport (MachoUnauthenticatedTransport transport)
    {
        Log.Information ($"Connection from proxy with nodeID {transport.Session.NodeID}");
        // first remove the transport from the unauthenticated list
        UnauthenticatedTransports.Remove (transport);

        // create the new client transport and store it somewhere
        MachoProxyTransport newTransport = new MachoProxyTransport (transport);

        if (ProxyTransports.TryGetValue (newTransport.Session.NodeID, out MachoProxyTransport original))
            original.AbortConnection ();

        ProxyTransports.Add (newTransport.Session.NodeID, newTransport);
        TransportList.Add (newTransport);

        OnProxyResolved?.Invoke (this, newTransport);
    }

    public void OnTransportTerminated (MachoTransport transport)
    {
        if (transport is not MachoUnauthenticatedTransport)
            TransportList.Remove (transport);

        switch (transport)
        {
            case MachoUnauthenticatedTransport:
                UnauthenticatedTransports.Remove (transport);
                break;

            case MachoClientTransport:
                ClientTransports.Remove (transport.Session.UserID);
                break;

            case MachoNodeTransport:
                NodeTransports.Remove (transport.Session.NodeID);
                break;

            case MachoProxyTransport:
                ProxyTransports.Remove (transport.Session.NodeID);
                break;
        }

        OnTransportRemoved?.Invoke (this, transport);
    }
}