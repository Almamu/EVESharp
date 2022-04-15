using System;
using System.Text.RegularExpressions;
using EVESharp.EVE.Packets;
using EVESharp.Node.Sessions;
using EVESharp.Node.Client.Notifications.Inventory;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Notifications;
using EVESharp.Node.Notifications.Nodes.Corps;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Server.Shared.Messages;
using EVESharp.Node.Services;
using EVESharp.Node.Services.Corporations;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;
using OnItemChange = EVESharp.Node.Notifications.Nodes.Inventory.OnItemChange;

namespace EVESharp.Node.Server.Node.Messages;

public class MessageProcessor : Shared.Messages.MessageProcessor
{
    public NotificationSender Notifications { get; }
    public ItemFactory        ItemFactory   { get; }
    public SystemManager      SystemManager { get; }

    public MessageProcessor (
        IMachoNet      machoNet,       ILogger             logger, NotificationSender notificationSender, ItemFactory itemFactory, SystemManager systemManager,
        ServiceManager serviceManager, BoundServiceManager boundServiceManager, RemoteServiceManager remoteServiceManager, PacketCallHelper packetCallHelper, SessionManager sessionManager
    ) : base (machoNet, logger, serviceManager, boundServiceManager, remoteServiceManager, packetCallHelper, itemFactory, systemManager, notificationSender, sessionManager, 100)
    {
        Notifications = notificationSender;
        ItemFactory   = itemFactory;
        SystemManager = systemManager;
    }

    protected override void HandleMessage (MachoMessage machoMessage)
    {
        switch (machoMessage.Packet.Type)
        {
            case PyPacket.PacketType.SESSIONCHANGENOTIFICATION:
                this.HandleSessionChangeNotification (machoMessage);

                break;
            case PyPacket.PacketType.CALL_REQ:
                LocalCallHandler.HandleCallReq (machoMessage);

                break;
            case PyPacket.PacketType.CALL_RSP:
                LocalCallHandler.HandleCallRsp (machoMessage);

                break;
            case PyPacket.PacketType.PING_REQ:
                this.HandlePingReq (machoMessage);

                break;
            case PyPacket.PacketType.NOTIFICATION:
                LocalNotificationHandler.HandleNotification (machoMessage);

                break;
        }
    }

    private void HandleSessionChangeNotification (MachoMessage machoMessage)
    {
        // ensure it comes from the correct node
        if (machoMessage.Packet.Source is not PyAddressNode source || machoMessage.Transport.Session.NodeID != source.NodeID)
            throw new Exception ("Received a session change notification from an unauthorized address");

        SessionChangeNotification scn = machoMessage.Packet.Payload;

        // get the characterID
        int characterID = machoMessage.Packet.OutOfBounds ["characterID"] as PyInteger;

        foreach ((int _, BoundService service) in BoundServiceManager.BoundServices)
            service.ApplySessionChange (characterID, scn.Changes);
    }

    private void HandlePingReq (MachoMessage machoMessage)
    {
        // alter package to include the times the data
        PyAddressClient source = machoMessage.Packet.Source as PyAddressClient;
        
        // this time should come from the stream packetizer or the socket itself
        // but there's no way we're adding time tracking for all the goddamned packets
        // so this should be sufficient
        PyTuple serverHandleMessage = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "server::handle_message"
        };

        PyTuple serverTurnaround = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "server::turnaround"
        };
        
        (machoMessage.Packet.Payload [0] as PyList)?.Add (serverHandleMessage);
        (machoMessage.Packet.Payload [0] as PyList)?.Add (serverTurnaround);

        // change to a response
        machoMessage.Packet.Type = PyPacket.PacketType.PING_RSP;

        // switch source and destination
        machoMessage.Packet.Source      = machoMessage.Packet.Destination;
        machoMessage.Packet.Destination = source;

        // queue the packet back
        MachoNet.QueueOutputPacket (machoMessage.Transport, machoMessage.Packet);
    }
}