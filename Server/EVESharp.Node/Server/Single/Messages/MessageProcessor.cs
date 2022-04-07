using System;
using EVESharp.Node.Network;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.Server.Shared.Messages;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Server.Single.Messages;

public class MessageProcessor : Shared.Messages.MessageProcessor
{
    public MessageProcessor (IMachoNet machoNet, ILogger logger, ServiceManager serviceManager, BoundServiceManager boundServiceManager) :
        base (machoNet, logger, serviceManager, boundServiceManager, 100) { }

    protected override void HandleMessage (MachoMessage machoMessage)
    {
        // check destinations to ensure the packet can be handled
        switch (machoMessage.Packet.Destination)
        {
            case PyAddressNode node:
                if (node.NodeID != MachoNet.NodeID)
                    throw new Exception ("Detected a packet to a node that is not us on a single-instance nodes");

                break;
            case PyAddressAny:
                break;

            case PyAddressBroadcast:
            case PyAddressClient:
                throw new Exception ("Detected a packet with a weird destination");
        }

        switch (machoMessage.Packet.Type)
        {
            case PyPacket.PacketType.CALL_REQ:
                LocalCallHandler.HandleCallReq (machoMessage);

                break;

            case PyPacket.PacketType.CALL_RSP:
                LocalCallHandler.HandleCallRsp (machoMessage);

                break;

            case PyPacket.PacketType.PING_REQ:
                LocalPingHandler.HandlePingReq (machoMessage);

                break;

            case PyPacket.PacketType.NOTIFICATION:
                this.HandleNotification (machoMessage);

                break;

            default:
                throw new NotImplementedException ("Only CallReq and PingReq packets can be handled in single-instance nodes");
        }
    }

    private void HandleNotification (MachoMessage machoMessage)
    {
        // check if there's any OOB useful data and remove the related objects
        if (machoMessage.Packet.OutOfBounds.TryGetValue ("OID-", out PyDictionary data) == false)
            return;

        foreach ((PyString guid, PyInteger refID) in data.GetEnumerable <PyString, PyInteger> ())
        {
            BoundServiceManager.ParseBoundServiceString (guid, out int nodeID, out int boundID);
            // cleanup the association if any
            machoMessage.Transport.Session.BoundObjects.Remove (boundID);
            // if the bound service is local, do it too
            BoundServiceManager.ClientHasReleasedThisObject (boundID, machoMessage.Transport.Session);
        }

        // update the nodes of interest list
        // TODO: FIND A BETTER WAY OF DOING THIS
        machoMessage.Transport.Session.NodesOfInterest.Clear ();

        foreach ((int boundID, long nodeID) in machoMessage.Transport.Session.BoundObjects)
            if (machoMessage.Transport.Session.NodesOfInterest.Contains (nodeID) == false)
                machoMessage.Transport.Session.NodesOfInterest.Add (nodeID);
    }
}