using System.Collections.Generic;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network;

public class MachoTransport
{
    /// <summary>
    /// The session associated with this transport
    /// </summary>
    public Session Session { get; }
    protected Channel Log { get; }
    /// <summary>
    /// The server transport that created it
    /// </summary>
    protected MachoServerTransport Server { get; }
    /// <summary>
    /// The underlying socket to send/receive data
    /// </summary>
    public EVEClientSocket Socket { get; }
    /// <summary>
    /// Queue of packets to be sent through the transport after the authentication happens
    /// </summary>
    protected Queue<PyDataType> PostAuthenticationQueue { get; } = new Queue<PyDataType>();

    public MachoTransport(MachoServerTransport transport, EVEClientSocket socket, Logger logger)
    {
        this.Session = new Session();
        this.Server = transport;
        this.Socket = socket;
        this.Log = logger.CreateLogChannel(socket.GetRemoteAddress());
    }

    public MachoTransport(MachoServerTransport transport, EVEClientSocket socket, Channel logger)
    {
        this.Session = new Session();
        this.Server = transport;
        this.Socket = socket;
        this.Log = logger;
    }

    public MachoTransport(MachoTransport source)
    {
        this.Session = source.Session;
        this.Log = source.Log;
        this.Socket = source.Socket;
        this.Server = source.Server;
    }

    /// <summary>
    /// Adds data to be sent after authentication happens
    /// </summary>
    /// <param name="data"></param>
    public void QueuePostAuthenticationPacket(PyDataType data)
    {
        this.PostAuthenticationQueue.Enqueue(data);
    }

    /// <summary>
    /// Flushes the post authentication packets queue and sends everything
    /// </summary>
    protected void SendPostAuthenticationPackets()
    {
        foreach (PyDataType packet in this.PostAuthenticationQueue)
            this.Socket.Send(packet);
    }
    
    public void AbortConnection()
    {
        this.Socket.GracefulDisconnect();

        // remove the transport from the list
        this.Server.OnTransportTerminated(this);
    }
}