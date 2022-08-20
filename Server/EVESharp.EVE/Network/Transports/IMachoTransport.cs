using System;
using EVESharp.Common.Network.Sockets;
using EVESharp.EVE.Sessions;
using Serilog;

namespace EVESharp.EVE.Network.Transports;

public interface IMachoTransport : IDisposable
{
    /// <summary>
    /// The session associated with this transport
    /// </summary>
    public Session Session { get; }
    /// <summary>
    /// Logger used by this transport
    /// </summary>
    public ILogger Log { get; }
    /// <summary>
    /// The underlying socket to send/receive data
    /// </summary>
    public IEVESocket Socket { get; }
    /// <summary>
    /// The MachoNet protocol version in use by this transport
    /// </summary>
    public IMachoNet MachoNet { get; }
    /// <summary>
    /// The transport manager that created this transport
    /// </summary>
    public ITransportManager TransportManager { get; }
    /// <summary>
    /// Event used when the transport is terminated
    /// </summary>
    public event Action <IMachoTransport> Terminated;
    /// <summary>
    /// Closes the underlying socket and frees it's resources
    /// </summary>
    public void Close ();
}