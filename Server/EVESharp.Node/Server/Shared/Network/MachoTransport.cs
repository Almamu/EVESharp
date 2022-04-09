using System.Collections.Generic;
using EVESharp.Common.Network;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Server.Shared.Network;

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
    /// <summary>
    /// Queue of packets to be sent through the transport after the authentication happens
    /// </summary>
    protected Queue <PyDataType> PostAuthenticationQueue { get; } = new Queue <PyDataType> ();

    public MachoTransport (IMachoNet machoNet, EVEClientSocket socket, ILogger logger)
    {
        Session  = new Session ();
        MachoNet = machoNet;
        Socket   = socket;
        Log      = logger;
    }

    public MachoTransport (MachoTransport source)
    {
        Session  = source.Session;
        Log      = source.Log;
        Socket   = source.Socket;
        MachoNet = source.MachoNet;
    }

    /// <summary>
    /// Adds data to be sent after authentication happens
    /// </summary>
    /// <param name="data"></param>
    public void QueuePostAuthenticationPacket (PyDataType data)
    {
        PostAuthenticationQueue.Enqueue (data);
    }

    /// <summary>
    /// Flushes the post authentication packets queue and sends everything
    /// </summary>
    protected void SendPostAuthenticationPackets ()
    {
        foreach (PyDataType packet in PostAuthenticationQueue)
            Socket.Send (packet);
    }

    public void AbortConnection ()
    {
        Socket.GracefulDisconnect ();

        // remove the transport from the list
        MachoNet.OnTransportTerminated (this);
    }
}