using EVESharp.EVE.Accounts;
using EVESharp.EVE.Messages;
using EVESharp.EVE.Messages.Processor;
using EVESharp.EVE.Messages.Queue;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Network.Transports;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.EVE.Network;

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
    public IQueueProcessor <LoginQueueEntry> LoginProcessor { get; }
    /// <summary>
    /// The message processor to use for this IMachoNet instance
    /// </summary>
    public IQueueProcessor <MachoMessage> MessageProcessor { get; set; }
    /// <summary>
    /// The transport manager in use for this IMachoNet instance
    /// </summary>
    public ITransportManager TransportManager { get; }
    public PyList<PyObjectData> LiveUpdates { get; }

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