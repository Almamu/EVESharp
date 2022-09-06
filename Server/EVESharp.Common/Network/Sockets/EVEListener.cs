using System;
using System.Net;
using System.Net.Sockets;

namespace EVESharp.Common.Network.Sockets;

public class EVEListener : IEVEListener
{
    public event Action <IEVESocket> ConnectionAccepted;
    public event Action <Exception>  Exception;
    private Socket                   Socket { get; }
    private int                      Port   { get; }

    public EVEListener (int port)
    {
        this.Port   = port;
        this.Socket = new Socket (AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        // ensure support for both ipv4 and ipv4
        this.Socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        // setup transfer buffers
        this.Socket.ReceiveBufferSize = 64 * 1024;
        this.Socket.SendBufferSize    = 64 * 1024;
    }

    public virtual void Listen ()
    {
        this.Socket.Bind (new IPEndPoint (IPAddress.IPv6Any, Port));
        this.Socket.Listen (20);
        
        // begin accepting connections too
        this.Socket.BeginAccept (AcceptCallback, this);
    }

    /// <summary>
    /// Fires the connection accepted event
    /// </summary>
    /// <param name="socket"></param>
    protected virtual void OnConnectionAccepted (IEVESocket socket)
    {
        this.ConnectionAccepted?.Invoke (socket);
    }

    private void AcceptCallback (IAsyncResult ar)
    {
        try
        {
            Socket    socket       = this.Socket.EndAccept (ar);
            EVESocket clientSocket = new EVESocket (socket);
            
            // begin accepting again
            this.Socket.BeginAccept (this.AcceptCallback, this);

            this.ConnectionAccepted (clientSocket);
        }
        catch (Exception e)
        {
            this.Exception (e);
        }
    }

    public void Close ()
    {
        this.Socket.Shutdown (SocketShutdown.Both);
        this.Socket.Close ();
    }
    
    public void Dispose ()
    {
        this.Socket.Dispose ();
    }
}