using System.Net.Http;
using EVESharp.Common.Logging;
using EVESharp.Database;
using EVESharp.EVE.Accounts;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Network.Transports;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.Server.Shared.Messages;
using EVESharp.Node.Server.Shared.Transports;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Server.Node;

public class MachoNet : IMachoNet
{
    private General              Configuration { get; }
    private MachoServerTransport Transport     { get; set; }
    private HttpClient           HttpClient    { get; }
    private IDatabaseConnection  Database      { get; }

    public MachoNet (
        IDatabaseConnection databaseConnection, HttpClient httpClient, ITransportManager transportManager, MessageProcessor <LoginQueueEntry> loginQueue,
        General             configuration,      ILogger    logger
    )
    {
        Database         = databaseConnection;
        HttpClient       = httpClient;
        LoginQueue       = loginQueue;
        TransportManager = transportManager;
        Configuration    = configuration;
        Log              = logger;
    }

    public long                               NodeID           { get; set; }
    public string                             Address          { get; set; }
    public RunMode                            Mode             => RunMode.Server;
    public ushort                             Port             => Configuration.MachoNet.Port;
    public ILogger                            Log              { get; }
    public string                             OrchestratorURL  => Configuration.Cluster.OrchestatorURL;
    public MessageProcessor <LoginQueueEntry> LoginQueue       { get; }
    public MessageProcessor <MachoMessage>    MessageProcessor { get; set; }
    public ITransportManager                  TransportManager { get; }
    public PyList <PyObjectData>              LiveUpdates      => Database.EveFetchLiveUpdates ();

    public void Initialize ()
    {
        Log.Fatal ("Starting MachoNet in node mode");
        Log.Error ("Starting MachoNet in node mode");
        Log.Warning ("Starting MachoNet in node mode");
        Log.Information ("Starting MachoNet in node mode");
        Log.Verbose ("Starting MachoNet in node mode");
        Log.Debug ("Starting MachoNet in node mode");

        // start the server socket
        Transport = new MachoServerTransport (Configuration.MachoNet.Port, HttpClient, this, Log.ForContext <MachoServerTransport> ("Listener"));
        Transport.Listen ();
    }

    public void QueueOutputPacket (MachoTransport origin, PyPacket packet)
    {
        // if origin is not null that means we're answering to "something"
        if (origin is not null)
        {
            origin.Socket.Send (packet);

            return;
        }

        // check destination, for anything not directed to a client we must notify all the proxies
        if (packet.Destination is not PyAddressClient client)
        {
            foreach ((long nodeID, MachoProxyTransport transport) in TransportManager.ProxyTransports)
                // TODO: OPTIMIZE THIS SO MARSHALING ONLY HAPPENS ONCE!
                transport.Socket.Send (packet);
        }
        else
        {
            // packet for a client means lookup
            // TODO: OPTIMIZE THIS WITH SOME KIND OF CACHE?
            long proxyNodeID = Database.CluResolveClientAddress (client.ClientID);

            if (TransportManager.ProxyTransports.TryGetValue (proxyNodeID, out MachoProxyTransport proxy) == false)
            {
                Log.Fatal ("Trying to send a packet without origin to a node we don't know... Aborting...");

                return;
            }

            proxy.Socket.Send (packet);
        }
    }

    public void QueueInputPacket (MachoTransport origin, PyPacket packet)
    {
        // add the packet to the processor
        MessageProcessor.Enqueue (
            new MachoMessage
            {
                Packet    = packet,
                Transport = origin
            }
        );
    }

    public void OnTransportTerminated (MachoTransport transport)
    {
        // remove the transport from the list
        TransportManager.OnTransportTerminated (transport);
    }
}