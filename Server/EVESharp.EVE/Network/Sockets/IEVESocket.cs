using System;
using EVESharp.Types;

namespace EVESharp.EVE.Network.Sockets;

public interface IEVESocket : IDisposable
{
    /// <summary>
    /// Event fired when new data is available on the socket
    /// </summary>
    public event Action <PyDataType> DataReceived;

    /// <summary>
    /// Event fired when the connection is lost (network problems, connection closed, anything...)
    /// </summary>
    public event Action ConnectionLost;

    /// <summary>
    /// Event fired when the connection generates any kind of exceptions
    /// </summary>
    public event Action <Exception> Exception;

    /// <summary>
    /// Address of the other side of the socket
    /// </summary>
    public string RemoteAddress { get; }

    /// <summary>
    /// Connects to the given address and port
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    public void Connect (string address, int port);

    /// <summary>
    /// Sends the given data through the socket
    /// </summary>
    /// <param name="data"></param>
    public void Send (PyDataType data);
    
    /// <summary>
    /// Closes the socket
    /// </summary>
    public void Close ();
}