using EVESharp.Common.Network.Messages;
using EVESharp.Node.Server.Shared.Handlers;
using EVESharp.Node.Services;
using Serilog;

namespace EVESharp.Node.Server.Shared.Messages;

public abstract class MessageProcessor : MessageProcessor <MachoMessage>
{
    protected IMachoNet           MachoNet            { get; }
    protected LocalCallHandler    LocalCallHandler    { get; }
    protected LocalPingHandler    LocalPingHandler    { get; }
    protected ServiceManager      ServiceManager      { get; }
    protected BoundServiceManager BoundServiceManager { get; }

    protected MessageProcessor (
        IMachoNet machoNet, ILogger logger, ServiceManager serviceManager, BoundServiceManager boundServiceManager, int numberOfThreads
    ) : base (logger, numberOfThreads)
    {
        MachoNet            = machoNet;
        ServiceManager      = serviceManager;
        BoundServiceManager = boundServiceManager;
        LocalCallHandler    = new LocalCallHandler (MachoNet, this, logger, serviceManager, boundServiceManager);
        LocalPingHandler    = new LocalPingHandler (MachoNet, this);

        // update the message processor for the macho net instance
        MachoNet.MessageProcessor = this;
    }
}