using EVESharp.Common.Network.Messages;
using EVESharp.Node.Accounts;
using EVESharp.Node.Database;
using EVESharp.Node.Server.Shared.Messages;
using EVESharp.Node.Server.Shared.Transports;
using EVESharp.PythonTypes.Types.Network;
using Serilog;

namespace EVESharp.Node.Server.Shared;

public interface IMachoNet
{
    /// <summary>
    /// The nodeID for this instance of MachoNet
    /// </summary>
    public long NodeID { get; set; }
    /// <summary>
    /// The address assigned by the Orchestrator
    /// </summary>
    public string Address { get; set; }
    /// <summary>
    /// Indicates in what mode the protocol is running in
    /// </summary>
    public RunMode Mode { get; }
    /// <summary>
    /// The port this instance of MachoNet uses
    /// </summary>
    public ushort Port { get; }
    /// <summary>
    /// The logger used by this MachoNet instance
    /// </summary>
    public ILogger Log { get; }
    /// <summary>
    /// The base URL for the Orchestrator API
    /// </summary>
    public string OrchestratorURL { get; }
    /// <summary>
    /// The login queue used for processing logins
    /// </summary>
    public LoginQueue LoginQueue { get; }
    /// <summary>
    /// The message processor to use for this IMachoNet instance
    /// </summary>
    public MessageProcessor <MachoMessage> MessageProcessor { get; set; }
    /// <summary>
    /// The transport manager in use for this IMachoNet instance
    /// </summary>
    public TransportManager TransportManager { get; }
    /// <summary>
    /// The general database
    /// </summary>
    public GeneralDB GeneralDB { get; }

    /// <summary>
    /// Initializes this macho net instance
    /// </summary>
    public void Initialize ();

    /// <summary>
    /// Queues a packet to be sent out
    /// </summary>
    /// <param name="origin">Where the packet originated (if any)</param>
    /// <param name="packet">The packet to queue</param>
    public void QueueOutputPacket (MachoTransport origin, PyPacket packet);

    /// <summary>
    /// Queues a packet to be sent out
    /// </summary>
    /// <param name="packet"></param>
    public void QueueOutputPacket (PyPacket packet)
    {
        this.QueueOutputPacket (null, packet);
    }

    /// <summary>
    /// Queues a packet to be processed and dispatched properly
    /// </summary>
    /// <param name="origin">Where the packet originated</param>
    /// <param name="packet">The packet to queue</param>
    public void QueueInputPacket (MachoTransport origin, PyPacket packet);

    /// <summary>
    /// Queues a packet to be processed and dispatched properly
    /// </summary>
    /// <param name="packet">The packet to queue</param>
    public void QueueInputPacket (PyPacket packet)
    {
        this.QueueInputPacket (null, packet);
    }

    /// <summary>
    /// Notifies MachoNet that a transport was closed
    /// </summary>
    /// <param name="transport"></param>
    public void OnTransportTerminated (MachoTransport transport);
}