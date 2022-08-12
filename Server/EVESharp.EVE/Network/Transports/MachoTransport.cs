using System.Collections.Generic;
using EVESharp.Common.Network;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.EVE.Network.Transports;

public class MachoTransport
{
    /// <summary>
    /// The session associated with this transport
    /// </summary>
    public Session Session { get; }
    public ILogger Log { get; }
    /// <summary>
    /// The underlying socket to send/receive data
    /// </summary>
    public EVEClientSocket Socket { get; }
    /// <summary>
    /// The MachoNet protocol version in use by this transport
    /// </summary>
    public IMachoNet MachoNet { get; }

    public MachoTransport (IMachoNet machoNet, EVEClientSocket socket, ILogger logger)
    {
        this.Session  = new Session ();
        this.MachoNet = machoNet;
        this.Socket   = socket;
        this.Log      = logger;
    }

    public MachoTransport (MachoTransport source)
    {
        this.Session  = source.Session;
        this.Log      = source.Log;
        this.Socket   = source.Socket;
        this.MachoNet = source.MachoNet;
    }

    public void AbortConnection ()
    {
        this.Socket.GracefulDisconnect ();

        // remove the transport from the list
        this.MachoNet.OnTransportTerminated (this);
    }
}