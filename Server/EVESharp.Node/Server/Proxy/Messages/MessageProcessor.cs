using System;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Network;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.Server.Shared.Messages;
using EVESharp.Node.Server.Shared.Transports;
using EVESharp.Node.Services;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;
using SessionManager = EVESharp.Node.Sessions.SessionManager;

namespace EVESharp.Node.Server.Proxy.Messages;

public class MessageProcessor : Shared.Messages.MessageProcessor
{
    public SessionManager SessionManager { get; }

    public MessageProcessor (
        IMachoNet machoNet, ILogger logger, ServiceManager serviceManager, BoundServiceManager boundServiceManager, SessionManager sessionManager
    ) :
        base (machoNet, logger, serviceManager, boundServiceManager, 100)
    {
        SessionManager = sessionManager;
    }

    protected override void HandleMessage (MachoMessage machoMessage)
    {
        if ((machoMessage.Packet.Destination is PyAddressNode node && node.NodeID == MachoNet.NodeID) ||
            machoMessage.Packet.Destination is PyAddressAny)
        {
            this.HandleAnyPacket (machoMessage);
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
                // TODO: SEND THIS TO A RANDOM NODEID INSTEAD OF HANDLING IT LOCALLY
                LocalPingHandler.HandlePingReq (machoMessage);

                break;
            case PyPacket.PacketType.NOTIFICATION:
                this.HandleNotification (machoMessage);

                break;
        }
    }

    private void HandleNotification (MachoMessage machoMessage)
    {
        PyPacket packet = machoMessage.Packet;

        // ensure the notification packet is valid
        // this packet is an internal one
        if (packet.Payload.Count != 2)
        {
            Log.Error ("Received ClusterController notification with the wrong format");

            return;
        }

        if (packet.Payload [0] is not PyString notification)
        {
            Log.Error ("Received ClusterController notification with the wrong format");

            return;
        }

        Log.Debug ($"Received a notification from ClusterController of type {notification.Value}");

        switch (notification)
        {
            case "UpdateSessionAttributes":
                this.HandleUpdateSessionAttributes (packet.Payload [1] as PyTuple);

                break;
            case "ClientHasDisconnected":
                this.HandleClientHasDisconnected (packet.Payload [1] as PyTuple, packet.OutOfBounds);

                break;
            default:
                Log.Fatal ("Received notification with the wrong format");

                break;
        }
    }

    private void HandleUpdateSessionAttributes (PyTuple payload)
    {
        // very simple version for now, should properly handle these sometime in the future
        PyString     idType    = payload [0] as PyString;
        PyInteger    id        = payload [1] as PyInteger;
        PyDictionary newValues = payload [2] as PyDictionary;

        SessionManager.PerformSessionUpdate (idType, id, Session.FromPyDictionary (newValues));
    }

    private void HandleClientHasDisconnected (PyTuple data, PyDictionary oob)
    {
        // unbind the player from all the services
        BoundServiceManager.OnClientDisconnected (Session.FromPyDictionary (oob ["Session"] as PyDictionary));
    }
}