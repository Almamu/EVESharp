using System;

namespace EVESharp.EVE.Network.Sockets;

public interface IEVEListener : IDisposable
{
    /// <summary>
    /// Event fired when a new connection is detected on the listener
    /// </summary>
    public event Action <IEVESocket> ConnectionAccepted;

    /// <summary>
    /// Event fired when the connection generates any kind of exceptions
    /// </summary>
    public event Action <Exception> Exception;
    
    /// <summary>
    /// Sets the listener to accept mode
    /// </summary>
    public void Listen ();

    /// <summary>
    /// Closes the listener and frees any of the pending data
    /// </summary>
    public void Close ();
}