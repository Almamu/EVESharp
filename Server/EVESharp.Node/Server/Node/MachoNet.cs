using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using EVESharp.Common.Database;
using EVESharp.Common.Logging;
using EVESharp.Common.Network.Messages;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Network;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.Server.Shared.Messages;
using EVESharp.PythonTypes.Types.Network;
using Serilog;
using AccountDB = EVESharp.Database.AccountDB;

namespace EVESharp.Node.Server.Node;

public class MachoNet : IMachoNet
{
    private General Configuration { get; init; }
    public long NodeID { get; set; }
    public string Address { get; set; }
    public RunMode Mode => RunMode.Server;
    public ushort Port => Configuration.MachoNet.Port;
    public ILogger Log { get; }
    public string OrchestratorURL => this.Configuration.Cluster.OrchestatorURL;
    public LoginQueue LoginQueue { get; }
    public MessageProcessor<MachoMessage> MessageProcessor { get; set; }
    public TransportManager TransportManager { get; }
    public GeneralDB GeneralDB { get; }
    private MachoServerTransport Transport { get; set; }
    private HttpClient HttpClient { get; }
    private DatabaseConnection Database { get; }

    public void Initialize()
    {
        Log.Fatal("Starting MachoNet in proxy mode");
        Log.Error("Starting MachoNet in proxy mode");
        Log.Warning("Starting MachoNet in proxy mode");
        Log.Information("Starting MachoNet in proxy mode");
        Log.Verbose("Starting MachoNet in proxy mode");
        Log.Debug("Starting MachoNet in proxy mode");
        
        // start the server socket
        this.Transport = new MachoServerTransport(this.Configuration.MachoNet.Port, this.HttpClient, this, Log.ForContext<MachoServerTransport>("Listener"));
        this.Transport.Listen();
    }

    public void QueueOutputPacket(MachoTransport origin, PyPacket packet)
    {
        // if origin is not null that means we're answering to "something"
        if (origin is not null)
        {
            origin.Socket.Send(packet);
            return;
        }
        
        // check destination, for anything not directed to a client we must notify all the proxies
        if (packet.Destination is not PyAddressClient client)
        {
            foreach ((long nodeID, MachoProxyTransport transport) in this.TransportManager.ProxyTransports)
            {
                // TODO: OPTIMIZE THIS SO MARSHALING ONLY HAPPENS ONCE!
                transport.Socket.Send(packet);
            }
        }
        else
        {
            // packet for a client means lookup
            // TODO: OPTIMIZE THIS WITH SOME KIND OF CACHE?
            long proxyNodeID = this.Database.Scalar<long>(
                AccountDB.RESOLVE_CLIENT_ADDRESS,
                new Dictionary<string, object>()
                {
                    {"_clientID", client.ClientID}
                }
            );

            if (this.TransportManager.ProxyTransports.TryGetValue(proxyNodeID, out MachoProxyTransport proxy) == false)
            {
                Log.Fatal("Trying to send a packet without origin to a node we don't know... Aborting...");
                return;
            }

            proxy.Socket.Send(packet);
        }
    }

    public void QueueInputPacket(MachoTransport origin, PyPacket packet)
    {
        // add the packet to the processor
        this.MessageProcessor.Enqueue(
            new MachoMessage
            {
                Packet = packet,
                Transport = origin
            }
        );
    }

    public void OnTransportTerminated(MachoTransport transport)
    {
        // remove the transport from the list
        this.TransportManager.OnTransportTerminated(transport);
    }

    public MachoNet(DatabaseConnection databaseConnection, GeneralDB generalDb, HttpClient httpClient, TransportManager transportManager, LoginQueue loginQueue, General configuration, ILogger logger)
    {
        this.Database = databaseConnection;
        this.GeneralDB = generalDb;
        this.HttpClient = httpClient;
        this.LoginQueue = loginQueue;
        this.TransportManager = transportManager;
        this.Configuration = configuration;
        this.Log = logger;
    }
}