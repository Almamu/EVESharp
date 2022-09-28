using System;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types.Network;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Services;
using Serilog;

namespace EVESharp.Node.Server.Single.Messages;

public class MessageQueue : Shared.Messages.MessageQueue
{
    public MessageQueue
    (
        IMachoNet machoNet, ILogger logger, ServiceManager serviceManager, IBoundServiceManager boundServiceManager, IRemoteServiceManager remoteServiceManager,
        PacketCallHelper packetCallHelper, INotificationSender notificationSender, IItems items, ISolarSystems solarSystems, ISessionManager sessionManager
    ) :
        base (
            machoNet, logger, serviceManager, boundServiceManager, remoteServiceManager, packetCallHelper, items, solarSystems,
            notificationSender, sessionManager
        ) { }

    public override void HandleMessage (MachoMessage machoMessage)
    {
        // check destinations to ensure the packet can be handled
        switch (machoMessage.Packet.Destination)
        {
            case PyAddressNode node:
                if (node.NodeID != MachoNet.NodeID)
                    throw new Exception ("Detected a packet to a node that is not us on a single-instance nodes");
                break;

            case PyAddressAny: break;

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
                LocalNotificationHandler.HandleNotification (machoMessage);
                break;

            default: throw new NotImplementedException ("Only CallReq and PingReq packets can be handled in single-instance nodes");
        }
    }
}