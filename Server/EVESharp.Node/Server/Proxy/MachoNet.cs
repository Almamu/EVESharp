using System;
using System.Net.Http;
using System.Reflection;
using EVESharp.Common.Logging;
using EVESharp.Common.Network.Messages;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Network;
using EVESharp.Node.Server.Proxy.Messages;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.Server.Shared.Messages;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;
using MessageProcessor = EVESharp.Node.Server.Proxy.Messages.MessageProcessor;

namespace EVESharp.Node.Server.Proxy;

public class MachoNet : IMachoNet
{
    private General Configuration { get; init; }
    public long NodeID { get; set; }
    public string Address { get; set; }
    public RunMode Mode => RunMode.Proxy;
    public ushort Port => Configuration.MachoNet.Port;
    public ILogger Log { get; }
    public string OrchestratorURL => this.Configuration.Cluster.OrchestatorURL;
    public LoginQueue LoginQueue { get; }
    public MessageProcessor<MachoMessage> MessageProcessor { get; set; }
    public TransportManager TransportManager { get; }
    public GeneralDB GeneralDB { get; }
    private MachoServerTransport Transport { get; set; }
    private HttpClient HttpClient { get; }

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
        // nothing else to do for now
    }

    public void QueueOutputPacket(MachoTransport origin, PyPacket packet)
    {
        // origin not being null means the packet came from a specific connection
        // and is a direct answer to it, so we can short-circuit through it
        // this can only happen if the destination is a client
        if (origin is MachoClientTransport && packet.Destination is PyAddressClient)
        {
            origin.Socket.Send(packet);
            return;
        }
        
        // TODO: OPTIMIZE THIS? ALL THE PACKETS ARE BEING MARSHALLED MULTIPLE TIMES
        // TODO: MAYBE IT'S BETTER TO DO IT ONCE AND SEND THE SAME DATA TO EVERYONE?
        // TODO: DO WE HAVE A REASON TO SEND A DIFFERENT PACKET TO EVERYONE?
        // packet queueing is simple on single instances
        switch (packet.Destination)
        {
            case PyAddressClient dest:
                this.SendPacketToClient(dest.ClientID, packet);
                break;
            case PyAddressBroadcast:
                this.SendBroadcastPacket(packet);
                break;
            case PyAddressNode dest:
                if (dest.NodeID == this.NodeID)
                    this.QueueInputPacket(origin, packet);
                else
                    this.SendPacketToNode(dest.NodeID, packet);
                break;
            case PyAddressAny:
                // a packet destinated to any node should be handled locally by us
                this.QueueInputPacket(origin, packet);
                break;
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
        // build the packet to be sent to everyone
        PyPacket clusterPacket = new PyPacket(PyPacket.PacketType.NOTIFICATION)
        {
            Source = new PyAddressNode(this.NodeID),
            Destination = new PyAddressBroadcast(transport.Session.NodesOfInterest, "nodeid"),
            Payload = new PyTuple(2) {[0] = "ClientHasDisconnected", [1] = new PyTuple(1) {[0] = transport.Session.UserID}},
            UserID = transport.Session.UserID,
            OutOfBounds = new PyDictionary() {["Session"] = transport.Session}
        };
        PyPacket localPacket = new PyPacket(PyPacket.PacketType.NOTIFICATION)
        {
            Source = new PyAddressNode(this.NodeID),
            Destination = new PyAddressNode(this.NodeID),
            Payload = new PyTuple(2) {[0] = "ClientHasDisconnected", [1] = new PyTuple(1) {[0] = transport.Session.UserID}},
            UserID = transport.Session.UserID,
            OutOfBounds = new PyDictionary() {["Session"] = transport.Session}
        };
        // tell all the nodes that we're dead now
        // HACK: UGH THAT CASTING IS UGLY! HOPEFULLY THIS CHANGES ON NEWER C# VERSIONS
        ((IMachoNet)this).QueueOutputPacket(clusterPacket);
        // queue the packet as input too so the proxy handles the disconnection too
        ((IMachoNet)this).QueueInputPacket(localPacket);
        // remove the transport from the list
        this.TransportManager.OnTransportTerminated(transport);
    }

    private void SendPacketToClient(int clientID, PyPacket packet)
    {
        if (this.TransportManager.ClientTransports.TryGetValue(clientID, out MachoClientTransport transport) == false)
            throw new Exception($"Cannot find a transport for client {clientID}");

        // send the data to the client
        transport.Socket.Send(packet);
    }

    private void SendPacketToNode(long nodeID, PyPacket packet)
    {
        if (this.TransportManager.NodeTransports.TryGetValue(nodeID, out MachoNodeTransport transport) == false)
            throw new Exception($"Cannot find a transport for node {nodeID}");
        
        // send the data to the node
        transport.Socket.Send(packet);
    }
    
    /// <summary>
    /// Reads the destination of a broadcast packet and queues it for everyone that should receive it
    /// </summary>
    /// <param name="packet">The packet to send</param>
    private void SendBroadcastPacket(PyPacket packet)
    {
        PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;

        if (dest.IDType == "*multicastID")
        {
            this.SendMulticastBroadcastPacket(packet);
        }
        else if (dest.IDType.Value.Contains('&') == true)
        {
            this.SendComplexBroadcastPacket(packet);
        }
        else
        {
            this.SendSimpleBroadcastPacket(packet);
        }
    }

    private void SendSimpleBroadcastPacket(PyPacket packet)
    {
        PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;
        bool isOwnerID = dest.IDType == "ownerid";

        foreach (PyInteger id in dest.IDsOfInterest.GetEnumerable<PyInteger>())
        {
            // loop all transports, search for the idtype given and send it
            foreach (MachoTransport transport in this.TransportManager.TransportList)
            {
                if (isOwnerID == true)
                {
                    if (transport.Session.AllianceID == id ||
                        transport.Session.CharacterID == id ||
                        transport.Session.CorporationID == id)
                        transport.Socket.Send(packet);
                }
                else if (transport.Session[dest.IDType] == id)
                {
                    transport.Socket.Send(packet);
                }
            }
        }
    }

    private void SendComplexBroadcastPacket(PyPacket packet)
    {
        PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;
        
        // extract the actual ids used to identify the destination
        string[] criteria = dest.IDType.Value.Split('&');
        // determine if any of those IDs is an ownerID so we can take it into account
        bool[] isOwnerID = Array.ConvertAll(criteria, x => x == "ownerid");

        foreach (PyTuple id in dest.IDsOfInterest.GetEnumerable<PyTuple>())
        {
            // ignore invalid ids for now
            if (id.Count != criteria.Length)
                continue;

            foreach (MachoTransport transport in this.TransportManager.TransportList)
            {
                bool found = true;
                
                // validate all the values
                for (int i = 0; i < criteria.Length; i++)
                {
                    if (isOwnerID[i] == true)
                    {
                        if (transport.Session.AllianceID != id[i] &&
                            transport.Session.CharacterID != id[i] &&
                            transport.Session.CorporationID != id[i])
                        {
                            found = false;
                            break;
                        }
                    }
                    else if (transport.Session[criteria[i]] != id[i])
                    {
                        found = false;
                        break;
                    }
                }

                // notify the transport if all the rules matched
                if (found)
                    transport.Socket.Send(packet);
            }
        }
    }

    private void SendMulticastBroadcastPacket(PyPacket packet)
    {
        throw new NotImplementedException();
    }

    public MachoNet(GeneralDB generalDb, HttpClient httpClient, TransportManager transportManager, LoginQueue loginQueue, General configuration, ILogger logger)
    {
        this.GeneralDB = generalDb;
        this.HttpClient = httpClient;
        this.LoginQueue = loginQueue;
        this.TransportManager = transportManager;
        this.Configuration = configuration;
        this.Log = logger;
    }
}