using System;
using System.Net;
using Serilog;

namespace EVESharp.Common.Network;

public class EVEServerSocket : EVESocket
{
    public ILogger Log  { get; }
    public int     Port { get; }

    public EVEServerSocket (int port, ILogger logChannel)
    {
        Log  = logChannel;
        Port = port;
    }

    public void Listen ()
    {
        // bind the socket to the correct endpoint
        Socket.Bind (new IPEndPoint (IPAddress.IPv6Any, Port));
        Socket.Listen (20);
    }

    public void BeginAccept (AsyncCallback callback)
    {
        Socket.BeginAccept (callback, this);
    }

    public EVEClientSocket EndAccept (IAsyncResult asyncResult)
    {
        return new EVEClientSocket (Socket.EndAccept (asyncResult), Log);
    }

    public override void GracefulDisconnect ()
    {
        // graceful disconnect is the same as forcefully in a listening socket
        this.ForcefullyDisconnect ();
    }

    protected override void DefaultExceptionHandler (Exception ex)
    {
        Log.Error ("Unhandled exception on underlying socket:");
        Log.Error (ex.Message);
    }
}