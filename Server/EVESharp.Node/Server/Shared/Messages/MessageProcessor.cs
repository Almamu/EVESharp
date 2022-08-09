using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Server.Shared.Handlers;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Services;
using Serilog;

namespace EVESharp.Node.Server.Shared.Messages;

public abstract class MessageProcessor : MessageProcessor <MachoMessage>
{
    protected IMachoNet                MachoNet                 { get; }
    protected IItems                   Items                    { get; }
    protected ISolarSystems            SolarSystems             { get; }
    protected INotificationSender      Notifications            { get; }
    protected LocalCallHandler         LocalCallHandler         { get; }
    protected LocalPingHandler         LocalPingHandler         { get; }
    protected LocalNotificationHandler LocalNotificationHandler { get; }
    protected ServiceManager           ServiceManager           { get; }
    protected BoundServiceManager      BoundServiceManager      { get; }
    protected ISessionManager          SessionManager           { get; }
    protected IRemoteServiceManager     RemoteServiceManager     { get; }

    protected MessageProcessor
    (
        IMachoNet machoNet, ILogger logger, ServiceManager serviceManager, BoundServiceManager boundServiceManager, IRemoteServiceManager remoteServiceManager,
        PacketCallHelper packetCallHelper, IItems items, ISolarSystems solarSystems, INotificationSender notifications, ISessionManager sessionManager,
        int numberOfThreads
    ) : base (logger, numberOfThreads)
    {
        MachoNet             = machoNet;
        ServiceManager       = serviceManager;
        BoundServiceManager  = boundServiceManager;
        this.Items           = items;
        this.SolarSystems    = solarSystems;
        Notifications        = notifications;
        RemoteServiceManager = remoteServiceManager;
        SessionManager       = sessionManager;
        LocalCallHandler     = new LocalCallHandler (MachoNet, logger, ServiceManager, BoundServiceManager, RemoteServiceManager, packetCallHelper);
        LocalPingHandler     = new LocalPingHandler (MachoNet);

        LocalNotificationHandler = new LocalNotificationHandler (
            MachoNet, logger, ServiceManager, BoundServiceManager, this.Items, this.SolarSystems, Notifications, SessionManager
        );

        // update the message processor for the macho net instance
        MachoNet.MessageProcessor = this;
    }
}