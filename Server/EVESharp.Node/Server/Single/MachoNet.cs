using System;
using System.Linq;
using System.Net.Http;
using EVESharp.Common.Constants;
using EVESharp.Common.Database;
using EVESharp.Common.Logging;
using EVESharp.Common.Network.Messages;
using EVESharp.Database;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Notifications;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.Server.Shared.Messages;
using EVESharp.Node.Server.Shared.Transports;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;
using AccountDB = EVESharp.Database.AccountDB;
using ItemDB = EVESharp.Database.ItemDB;

namespace EVESharp.Node.Server.Single;

public class MachoNet : IMachoNet
{
    private General              Configuration { get; }
    private IDatabaseConnection  Database      { get; }
    private MachoServerTransport Transport     { get; set; }
    private HttpClient           HttpClient    { get; }

    public MachoNet (
        GeneralDB generalDb,     HttpClient httpClient, IDatabaseConnection databaseConnection, TransportManager transportManager, LoginQueue loginQueue,
        General   configuration, ILogger    logger
    )
    {
        GeneralDB        = generalDb;
        HttpClient       = httpClient;
        Database         = databaseConnection;
        LoginQueue       = loginQueue;
        TransportManager = transportManager;
        Configuration    = configuration;
        Log              = logger;
    }

    public long                            NodeID           { get; set; } = Network.PROXY_NODE_ID;
    public string                          Address          { get; set; }
    public RunMode                         Mode             => RunMode.Single;
    public ushort                          Port             => Configuration.MachoNet.Port;
    public ILogger                         Log              { get; }
    public string                          OrchestratorURL  => Configuration.Cluster.OrchestatorURL;
    public LoginQueue                      LoginQueue       { get; }
    public MessageProcessor <MachoMessage> MessageProcessor { get; set; }
    public TransportManager                TransportManager { get; }
    public GeneralDB                       GeneralDB        { get; }

    public void Initialize ()
    {
        Log.Fatal ("Starting MachoNet in single mode");
        Log.Error ("Starting MachoNet in single mode");
        Log.Warning ("Starting MachoNet in single mode");
        Log.Information ("Starting MachoNet in single mode");
        Log.Verbose ("Starting MachoNet in single mode");
        Log.Debug ("Starting MachoNet in single mode");

        Database.InvClearNodeAssociation ();
        Database.CluResetClientAddresses ();
        Database.CluCleanup ();
        Database.CluRegisterSingleNode (NodeID);

        // start the server socket
        Transport = new MachoServerTransport (Configuration.MachoNet.Port, HttpClient, this, Log.ForContext <MachoServerTransport> ("Listener"));
        Transport.Listen ();
    }

    public void QueueOutputPacket (MachoTransport origin, PyPacket packet)
    {
        // origin not being null means the packet came from a specific connection
        // and is a direct answer to it, so we can short-circuit through it
        // this can only happen if the destination is a client
        if (origin is not null && packet.Destination is PyAddressClient)
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
            case PyAddressNode:
            case PyAddressAny:
                // a packet destinated to any node should be handled locally by us
                this.QueueInputPacket (origin, packet);

                break;
        }
    }

    public void QueueInputPacket (MachoTransport origin, PyPacket packet)
    {
        // add the packet to the processor
        MessageProcessor?.Enqueue (
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

    private void SendPacketToClient (int clientID, PyPacket packet)
    {
        if (TransportManager.ClientTransports.TryGetValue (clientID, out MachoClientTransport transport) == false)
            throw new Exception ($"Cannot find a transport for client {clientID}");

        // send the data to the client
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
        if (NotificationSender.NotificationComparison.TryGetValue (dest.IDType, out ComparisonType comparison) == false)
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
                        else if (isOwnerID && transport.Session.AllianceID != id &&
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
                if (NotificationSender.NotificationComparison.TryGetValue (x, out ComparisonType result) == false)
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
                            else if (isOwnerID[i] && 
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