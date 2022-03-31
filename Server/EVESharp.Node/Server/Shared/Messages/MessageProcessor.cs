using System.Runtime.CompilerServices;
using EVESharp.Common.Network.Messages;
using EVESharp.Node.Network;
using EVESharp.Node.Server.Shared.Handlers;
using Serilog;

namespace EVESharp.Node.Server.Shared.Messages;

public abstract class MessageProcessor : MessageProcessor<MachoMessage>
{
    protected IMachoNet MachoNet { get; }
    protected LocalCallHandler LocalCallHandler { get; }
    protected LocalPingHandler LocalPingHandler { get; }
    protected ServiceManager ServiceManager { get; }
    protected BoundServiceManager BoundServiceManager { get; }
    protected MessageProcessor(IMachoNet machoNet, ILogger logger, ServiceManager serviceManager, BoundServiceManager boundServiceManager, int numberOfThreads) : base(logger, numberOfThreads)
    {
        this.MachoNet = machoNet;
        this.ServiceManager = serviceManager;
        this.BoundServiceManager = boundServiceManager;
        this.LocalCallHandler = new LocalCallHandler(this.MachoNet, this, logger, serviceManager, boundServiceManager);
        this.LocalPingHandler = new LocalPingHandler(this.MachoNet, this);
        
        // update the message processor for the macho net instance
        this.MachoNet.MessageProcessor = this;
    }
}