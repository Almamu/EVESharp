using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Services;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Server.Proxy.Messages;

public class MessageProcessor : Shared.Messages.MessageProcessor
{

    public MessageProcessor (
        IMachoNet machoNet, ILogger logger, ServiceManager serviceManager, BoundServiceManager boundServiceManager, ISessionManager sessionManager, RemoteServiceManager remoteServiceManager, PacketCallHelper packetCallHelper, INotificationSender notificationSender, IItems items, ISolarSystems solarSystems
    ) :
        base (machoNet, logger, serviceManager, boundServiceManager, remoteServiceManager, packetCallHelper, items, solarSystems, notificationSender, sessionManager, 100)
    {
    }

    protected override void HandleMessage (MachoMessage machoMessage)
    {
        if ((machoMessage.Packet.Destination is PyAddressNode node && node.NodeID == MachoNet.NodeID) ||
            machoMessage.Packet.Destination is PyAddressAny)
        {
            this.HandleAnyPacket (machoMessage);
        }
        else if (machoMessage.Packet.Type == PyPacket.PacketType.PING_RSP && machoMessage.Transport is MachoNodeTransport)
        {
            this.HandlePingRsp (machoMessage);            
        }
        else
        {
            if (machoMessage.Packet.Source is PyAddressClient)
            {
                machoMessage.Packet.OutOfBounds ??= new PyDictionary ();
                // ensure the session is attached
                machoMessage.Packet.OutOfBounds ["Session"] = machoMessage.Transport.Session;
            }

            MachoNet.QueueOutputPacket (machoMessage.Transport, machoMessage.Packet);
        }

        // null out of bounds means no data to handle
        if (machoMessage.Packet.OutOfBounds is null)
            return;

        // there's some situations where we need to check OOB and handle it
        if (machoMessage.Transport is MachoClientTransport &&
            machoMessage.Packet.OutOfBounds.TryGetValue ("OID-", out PyDictionary deleted))
        {
            foreach ((PyString guid, PyInteger refID) in deleted.GetEnumerable <PyString, PyInteger> ())
            {
                BoundServiceManager.ParseBoundServiceString (guid, out int nodeID, out int boundID);
                // cleanup the association if any
                machoMessage.Transport.Session.BoundObjects.Remove (boundID);
                // if the bound service is local, do it too
                BoundServiceManager.ClientHasReleasedThisObject (boundID, machoMessage.Transport.Session);
            }

            // update node list
            machoMessage.Transport.Session.NodesOfInterest.Clear ();

            foreach ((int boundID, long nodeID) in machoMessage.Transport.Session.BoundObjects)
                if (machoMessage.Transport.Session.NodesOfInterest.Contains (nodeID) == false)
                    machoMessage.Transport.Session.NodesOfInterest.Add (nodeID);
        }
        else if (machoMessage.Transport is MachoNodeTransport &&
                 machoMessage.Packet.OutOfBounds.TryGetValue ("OID+", out PyList added))
        {
            PyAddressClient destination = machoMessage.Packet.Destination as PyAddressClient;

            // get the client we have to register them into
            if (MachoNet.TransportManager.ClientTransports.TryGetValue (destination.ClientID, out MachoClientTransport transport) == false)
                throw new Exception ("Bound services to an unknown client");

            // new ids registered, add to the list and ensure they're recorded
            foreach (PyTuple entry in added.GetEnumerable <PyTuple> ())
            {
                if (entry.Count != 2)
                    continue;

                PyString  guid  = entry [0] as PyString;
                PyInteger refID = entry [1] as PyInteger;

                BoundServiceManager.ParseBoundServiceString (guid, out int nodeID, out int boundID);
                transport.Session.BoundObjects [boundID] = nodeID;

                if (transport.Session.NodesOfInterest.Contains (nodeID) == false)
                    transport.Session.NodesOfInterest.Add (nodeID);
            }
        }
    }

    private void HandleAnyPacket (MachoMessage machoMessage)
    {
        switch (machoMessage.Packet.Type)
        {
            case PyPacket.PacketType.CALL_REQ:
                LocalCallHandler.HandleCallReq (machoMessage);
                break;

            case PyPacket.PacketType.CALL_RSP:
                LocalCallHandler.HandleCallRsp (machoMessage);
                break;

            case PyPacket.PacketType.PING_REQ:
                this.HandlePingReq (machoMessage);
                break;

            case PyPacket.PacketType.PING_RSP:
                this.HandlePingRsp (machoMessage);
                break;

            case PyPacket.PacketType.NOTIFICATION:
                LocalNotificationHandler.HandleNotification (machoMessage);
                break;

        }
    }

    private void HandlePingReq (MachoMessage machoMessage)
    {
        if (MachoNet.TransportManager.NodeTransports.Count == 0)
        {
            LocalPingHandler.HandlePingReq (machoMessage);

            return;
        }

        // TODO: CHECK THIS FOR PERFORMANCE?!
        List <MachoNodeTransport> nodeTransports = MachoNet.TransportManager.NodeTransports.Values.ToList ();

        int index = Random.Shared.Next(0, nodeTransports.Count);
        

        // this time should come from the stream packetizer or the socket itself
        // but there's no way we're adding time tracking for all the goddamned packets
        // so this should be sufficient
        PyTuple handleMessage = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "proxy::handle_message"
        };

        PyTuple writing = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "proxy::writing"
        };
        
        (machoMessage.Packet.Payload [0] as PyList)?.Add (handleMessage);
        (machoMessage.Packet.Payload [0] as PyList)?.Add (writing);
        
        // relay it to the node
        nodeTransports [index].Socket.Send (machoMessage.Packet);
    }

    private void HandlePingRsp (MachoMessage machoMessage)
    {
        // this time should come from the stream packetizer or the socket itself
        // but there's no way we're adding time tracking for all the goddamned packets
        // so this should be sufficient
        PyTuple handleMessage = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "proxy::handle_message"
        };

        PyTuple writing = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "proxy::writing"
        };
        
        (machoMessage.Packet.Payload [0] as PyList)?.Add (handleMessage);
        (machoMessage.Packet.Payload [0] as PyList)?.Add (writing);
        
        // queue it back so it now goes into the proper destination
        MachoNet.QueueOutputPacket (machoMessage.Packet);
    }
}