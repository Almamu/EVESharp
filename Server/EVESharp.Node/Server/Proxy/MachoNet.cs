using System;
using System.Linq;
using System.Net.Http;
using EVESharp.Common.Logging;
using EVESharp.Database;
using EVESharp.EVE.Accounts;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Notifications;
using EVESharp.Node.Configuration;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Server.Proxy;

public class MachoNet : IMachoNet
{
    private General              Configuration { get; }
    private MachoServerTransport Transport     { get; set; }
    private IDatabaseConnection  Database      { get; }

    public MachoNet (
        ITransportManager transportManager, MessageProcessor <LoginQueueEntry> loginQueue, General configuration, ILogger logger, IDatabaseConnection databaseConnection
    )
    {
        LoginQueue       = loginQueue;
        TransportManager = transportManager;
        Configuration    = configuration;
        Log              = logger;
        Database         = databaseConnection;
    }

    public long                               NodeID           { get; set; }
    public string                             Address          { get; set; }
    public RunMode                            Mode             => RunMode.Proxy;
    public ushort                             Port             => Configuration.MachoNet.Port;
    public ILogger                            Log              { get; }
    public string                             OrchestratorURL  => Configuration.Cluster.OrchestatorURL;
    public MessageProcessor <LoginQueueEntry> LoginQueue       { get; }
    public MessageProcessor <MachoMessage>    MessageProcessor { get; set; }
    public ITransportManager                  TransportManager { get; }
    public PyList <PyObjectData>              LiveUpdates      => Database.EveFetchLiveUpdates ();

    public void Initialize ()
    {
        Log.Fatal ("Starting MachoNet in proxy mode");
        Log.Error ("Starting MachoNet in proxy mode");
        Log.Warning ("Starting MachoNet in proxy mode");
        Log.Information ("Starting MachoNet in proxy mode");
        Log.Verbose ("Starting MachoNet in proxy mode");
        Log.Debug ("Starting MachoNet in proxy mode");

        // start the server socket
        this.TransportManager.OpenServerTransport (this, Configuration.MachoNet).Listen ();
        // nothing else to do for now
    }

    public void QueueOutputPacket (MachoTransport origin, PyPacket packet)
    {
        // origin not being null means the packet came from a specific connection
        // and is a direct answer to it, so we can short-circuit through it
        // this can only happen if the destination is a client
        if (origin is MachoClientTransport && packet.Destination is PyAddressClient)
        {
            origin.Socket.Send (packet);

            return;
        }

        // TODO: OPTIMIZE THIS? ALL THE PACKETS ARE BEING MARSHALLED MULTIPLE TIMES
        // TODO: MAYBE IT'S BETTER TO DO IT ONCE AND SEND THE SAME DATA TO EVERYONE?
        // TODO: DO WE HAVE A REASON TO SEND A DIFFERENT PACKET TO EVERYONE?
        // packet queueing is simple on single instances
        switch (packet.Destination)
        {
            case PyAddressClient dest:
                this.SendPacketToClient (dest.ClientID, packet);
                break;

            case PyAddressBroadcast:
                this.SendBroadcastPacket (packet);
                break;

            case PyAddressNode dest:
                if (dest.NodeID == NodeID)
                    this.QueueInputPacket (origin, packet);
                else
                    this.SendPacketToNode (dest.NodeID, packet);
                break;

            case PyAddressAny:
                // a packet destinated to any node should be handled locally by us
                this.QueueInputPacket (origin, packet);
                break;

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
        // build the packet to be sent to everyone
        PyPacket clusterPacket = new PyPacket (PyPacket.PacketType.NOTIFICATION)
        {
            Source      = new PyAddressNode (NodeID),
            Destination = new PyAddressBroadcast (transport.Session.NodesOfInterest, "nodeid"),
            Payload = new PyTuple (2)
            {
                [0] = "ClientHasDisconnected",
                [1] = new PyTuple (1) {[0] = transport.Session.UserID}
            },
            UserID      = transport.Session.UserID,
            OutOfBounds = new PyDictionary {["Session"] = transport.Session}
        };
        PyPacket localPacket = new PyPacket (PyPacket.PacketType.NOTIFICATION)
        {
            Source      = new PyAddressNode (NodeID),
            Destination = new PyAddressNode (NodeID),
            Payload = new PyTuple (2)
            {
                [0] = "ClientHasDisconnected",
                [1] = new PyTuple (1) {[0] = transport.Session.UserID}
            },
            UserID      = transport.Session.UserID,
            OutOfBounds = new PyDictionary {["Session"] = transport.Session}
        };
        // tell all the nodes that we're dead now
        // HACK: UGH THAT CASTING IS UGLY! HOPEFULLY THIS CHANGES ON NEWER C# VERSIONS
        ((IMachoNet) this).QueueOutputPacket (clusterPacket);
        // queue the packet as input too so the proxy handles the disconnection too
        ((IMachoNet) this).QueueInputPacket (localPacket);
        // remove the transport from the list
        TransportManager.OnTransportTerminated (transport);
    }

    private void SendPacketToClient (int clientID, PyPacket packet)
    {
        if (TransportManager.ClientTransports.TryGetValue (clientID, out MachoClientTransport transport) == false)
            throw new Exception ($"Cannot find a transport for client {clientID}");

        // send the data to the client
        transport.Socket.Send (packet);
    }

    private void SendPacketToNode (long nodeID, PyPacket packet)
    {
        if (TransportManager.NodeTransports.TryGetValue (nodeID, out MachoNodeTransport transport) == false)
            throw new Exception ($"Cannot find a transport for node {nodeID}");

        // send the data to the node
        transport.Socket.Send (packet);
    }

    /// <summary>
    /// Reads the destination of a broadcast packet and queues it for everyone that should receive it
    /// </summary>
    /// <param name="packet">The packet to send</param>
    private void SendBroadcastPacket (PyPacket packet)
    {
        PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;

        if (dest.IDType == "*multicastID")
            this.SendMulticastBroadcastPacket (packet);
        else if (dest.IDType.Value.Contains ('&'))
            this.SendComplexBroadcastPacket (packet);
        else
            this.SendSimpleBroadcastPacket (packet);
    }

    private void SendSimpleBroadcastPacket (PyPacket packet)
    {
        PyAddressBroadcast dest      = packet.Destination as PyAddressBroadcast;
        bool               isOwnerID = dest.IDType == "ownerid";

        // determine how the values have to be checked
        if (INotificationSender.NotificationComparison.TryGetValue (dest.IDType, out ComparisonType comparison) == false)
            comparison = ComparisonType.Equality;

        foreach (PyInteger id in dest.IDsOfInterest.GetEnumerable <PyInteger> ())
        {
            // loop all transports, search for the idtype given and send it
            foreach (MachoTransport transport in TransportManager.TransportList)
            {
                switch (comparison)
                {
                    case ComparisonType.Bitmask:
                        // bitmask cannot be owner ids
                        if (transport.Session [dest.IDType] is not PyInteger val)
                            continue;
                        if ((val & id) != id)
                            continue;
                        break;
                    case ComparisonType.Equality:
                        if (!isOwnerID && transport.Session [dest.IDType] != id)
                            continue;
                        if (isOwnerID && transport.Session.AllianceID != id &&
                            transport.Session.CharacterID != id &&
                            transport.Session.CorporationID != id)
                            continue;
                        break;
                }

                transport.Socket.Send (packet);
            }
        }
    }

    private void SendComplexBroadcastPacket (PyPacket packet)
    {
        PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;

        // extract the actual ids used to identify the destination
        string [] criteria = dest.IDType.Value.Split ('&');
        // detect how the comparisons have to be performed
        ComparisonType [] comparison = criteria.Select (
            x =>
            {
                if (INotificationSender.NotificationComparison.TryGetValue (x, out ComparisonType result) == false)
                    result = ComparisonType.Equality;

                return result;
            }
        ).ToArray ();
        // determine if any of those IDs is an ownerID so we can take it into account
        bool [] isOwnerID = Array.ConvertAll (criteria, x => x == "ownerid");

        foreach (PyTuple id in dest.IDsOfInterest.GetEnumerable <PyTuple> ())
        {
            // ignore invalid ids for now
            if (id.Count != criteria.Length)
                continue;

            foreach (MachoTransport transport in TransportManager.TransportList)
            {
                bool found = true;

                // validate all the values
                for (int i = 0; i < criteria.Length; i++)
                {
                    switch (comparison[i])
                    {
                        case ComparisonType.Bitmask:
                            // bitmask cannot be owner ids
                            if (transport.Session [criteria [i]] is not PyInteger val)
                                break;
                            if (id [i] is not PyInteger val2)
                                break;
                            if ((val & val2) != val2)
                                break;
                            continue;
                        case ComparisonType.Equality:
                            if (!isOwnerID[i] && transport.Session [criteria[i]] != id [i])
                                break;
                            if (isOwnerID[i] && 
                                transport.Session.AllianceID != id [i] &&
                                transport.Session.CharacterID != id [i] &&
                                transport.Session.CorporationID != id [i])
                                break;
                            continue;
                    }

                    // if nothing matches we'll fall through to this one
                    found = false;
                    break;
                }

                // notify the transport if all the rules matched
                if (found)
                    transport.Socket.Send (packet);
            }
        }
    }

    private void SendMulticastBroadcastPacket (PyPacket packet)
    {
        throw new NotImplementedException ();
    }
}