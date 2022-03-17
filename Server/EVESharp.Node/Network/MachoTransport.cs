using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.EVE.Sessions;

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

    public MachoTransport(MachoServerTransport transport, EVEClientSocket socket, Logger logger)
    {
        this.Session = new Session();
        this.Server = transport;
        this.Socket = socket;
        this.Log = logger.CreateLogChannel(socket.GetRemoteAddress());
    }

    public MachoTransport(MachoTransport source)
    {
        this.Session = source.Session;
        this.Log = source.Log;
        this.Socket = source.Socket;
        this.Server = source.Server;
    }
        
    public void AbortConnection()
    {
        this.Socket.GracefulDisconnect();

        // remove the transport from the list
        this.Server.OnTransportTerminated(this);
    }
}