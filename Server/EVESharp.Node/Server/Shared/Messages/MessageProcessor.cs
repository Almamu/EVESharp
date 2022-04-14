using EVESharp.Common.Network.Messages;
using EVESharp.Node.Inventory;
using EVESharp.Node.Notifications;
using EVESharp.Node.Server.Shared.Handlers;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Services;
using EVESharp.Node.Sessions;
using Serilog;

namespace EVESharp.Node.Server.Shared.Messages;

public abstract class MessageProcessor : MessageProcessor <MachoMessage>
{
    protected IMachoNet                MachoNet                 { get; }
    protected ItemFactory              ItemFactory              { get; }
    protected SystemManager            SystemManager            { get; }
    protected NotificationSender       Notifications            { get; }
    protected LocalCallHandler         LocalCallHandler         { get; }
    protected LocalPingHandler         LocalPingHandler         { get; }
    protected LocalNotificationHandler LocalNotificationHandler { get; }
    protected ServiceManager           ServiceManager           { get; }
    protected BoundServiceManager      BoundServiceManager      { get; }
    protected SessionManager           SessionManager           { get; }
    protected RemoteServiceManager     RemoteServiceManager     { get; }

    protected MessageProcessor (
        IMachoNet machoNet, ILogger logger, ServiceManager serviceManager, BoundServiceManager boundServiceManager, RemoteServiceManager remoteServiceManager, PacketCallHelper packetCallHelper, ItemFactory itemFactory, SystemManager systemManager, NotificationSender notifications, SessionManager sessionManager, int numberOfThreads
    ) : base (logger, numberOfThreads)
    {
        MachoNet                 = machoNet;
        ServiceManager           = serviceManager;
        BoundServiceManager      = boundServiceManager;
        ItemFactory              = itemFactory;
        SystemManager            = systemManager;
        Notifications            = notifications;
        RemoteServiceManager     = remoteServiceManager;
        SessionManager           = sessionManager;
        LocalCallHandler         = new LocalCallHandler (MachoNet, logger, ServiceManager, BoundServiceManager, RemoteServiceManager, packetCallHelper);
        LocalPingHandler         = new LocalPingHandler (MachoNet);
        LocalNotificationHandler = new LocalNotificationHandler (MachoNet, logger, ServiceManager, BoundServiceManager, ItemFactory, SystemManager, Notifications, SessionManager);

        // update the message processor for the macho net instance
        MachoNet.MessageProcessor = this;
    }
}