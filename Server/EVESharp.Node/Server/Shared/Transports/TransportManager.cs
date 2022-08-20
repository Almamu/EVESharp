using System;
using System.Collections.Generic;
using System.Net.Http;
using EVESharp.Common.Configuration;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.Common.Network.Sockets;
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
    public List <IMachoTransport> UnauthenticatedTransports { get; } = new List <IMachoTransport> ();
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
    public List <IMachoTransport> TransportList { get; } = new List <IMachoTransport> ();
    protected ILogger    Log        { get; }
    protected HttpClient HttpClient { get; }

    /// <summary>
    /// Event fired when a transport is removed
    /// </summary>
    public event Action <IMachoTransport> OnTransportRemoved;
    public event Action <MachoClientTransport> OnClientResolved;
    public event Action <MachoNodeTransport>   OnNodeResolved;
    public event Action <MachoProxyTransport>  OnProxyResolved;

    public TransportManager (HttpClient httpClient, ILogger logger)
    {
        Log        = logger;
        HttpClient = httpClient;
    }

    public virtual MachoServerTransport OpenServerTransport (IMachoNet machoNet, MachoNet configuration)
    {
        return this.ServerTransport = new MachoServerTransport (configuration.Port, machoNet, Log.ForContext <MachoServerTransport> ());
    }

    public virtual IMachoTransport NewTransport (IMachoNet machoNet, IEVESocket socket)
    {
        MachoUnauthenticatedTransport transport = new MachoUnauthenticatedTransport (
            machoNet, this.HttpClient, socket, Log.ForContext <MachoUnauthenticatedTransport> (socket.RemoteAddress)
        );

        this.PrepareTransport (transport);
        
        UnauthenticatedTransports.Add (transport);

        return transport;
    }

    private void PrepareTransport (IMachoTransport transport)
    {
        // set some events on the transport
        transport.OnTerminated += this.OnTransportTerminated;
    }
    
    /// <summary>
    /// Registers the given transport as a client's transport
    /// </summary>
    /// <param name="transport"></param>
    public void ResolveClientTransport (MachoUnauthenticatedTransport transport)
    {
        // first remove the transport from the unauthenticated list
        UnauthenticatedTransports.Remove (transport);

        // clear transport's callbacks
        transport.Dispose ();
        
        // create the new client transport and store it somewhere
        MachoClientTransport newTransport = new MachoClientTransport (transport);

        if (ClientTransports.Remove (newTransport.Session.UserID, out MachoClientTransport original))
            original.Close ();

        this.PrepareTransport (newTransport);
        
        ClientTransports.Add (newTransport.Session.UserID, newTransport);
        TransportList.Add (newTransport);

        OnClientResolved?.Invoke (newTransport);
    }

    public void ResolveNodeTransport (MachoUnauthenticatedTransport transport)
    {
        Log.Information ($"Connection from server with nodeID {transport.Session.NodeID}");
        // first remove the transport from the unauthenticated list
        UnauthenticatedTransports.Remove (transport);

        // clear transport's callbacks
        transport.Dispose ();

        // create the new client transport and store it somewhere
        MachoNodeTransport newTransport = new MachoNodeTransport (transport);

        if (NodeTransports.Remove (newTransport.Session.NodeID, out MachoNodeTransport original))
            original.Close ();

        this.PrepareTransport (newTransport);
        
        NodeTransports.Add (newTransport.Session.NodeID, newTransport);
        TransportList.Add (newTransport);
        
        OnNodeResolved?.Invoke (newTransport);
    }

    public void ResolveProxyTransport (MachoUnauthenticatedTransport transport)
    {
        Log.Information ($"Connection from proxy with nodeID {transport.Session.NodeID}");
        // first remove the transport from the unauthenticated list
        UnauthenticatedTransports.Remove (transport);

        // clear transport's callbacks
        transport.Dispose ();

        // create the new client transport and store it somewhere
        MachoProxyTransport newTransport = new MachoProxyTransport (transport);

        if (ProxyTransports.Remove (newTransport.Session.NodeID, out MachoProxyTransport original))
            original.Close ();

        this.PrepareTransport (newTransport);
        
        ProxyTransports.Add (newTransport.Session.NodeID, newTransport);
        TransportList.Add (newTransport);

        OnProxyResolved?.Invoke (newTransport);
    }

    private void OnTransportTerminated (IMachoTransport transport)
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

        // close the transport and free any resources left
        transport.Close ();
        
        OnTransportRemoved?.Invoke (transport);
    }
}