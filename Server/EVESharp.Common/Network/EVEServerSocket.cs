using System;
using System.Net;
using System.Net.Sockets;
using EVESharp.Common.Logging;

namespace EVESharp.Common.Network
{
    public class EVEServerSocket : EVESocket
    {
        public Channel Log { get; }
        public int Port { get; }

        public EVEServerSocket(int port, Channel logChannel)
        {
            this.Log = logChannel;
            this.Port = port;
        }

        public void Listen()
        {
            // bind the socket to the correct endpoint
            this.Socket.Bind(new IPEndPoint(IPAddress.IPv6Any, this.Port));
            this.Socket.Listen(20);
        }

        public void BeginAccept(AsyncCallback callback)
        {
            this.Socket.BeginAccept(callback, this);
        }

        public EVEClientSocket EndAccept(IAsyncResult asyncResult)
        {
            return new EVEClientSocket(this.Socket.EndAccept(asyncResult), Log);
        }

        public override void GracefulDisconnect()
        {
            // graceful disconnect is the same as forcefully in a listening socket
            this.ForcefullyDisconnect();
        }

        protected override void DefaultExceptionHandler(Exception ex)
        {
            Log.Error("Unhandled exception on underlying socket:");
            Log.Error(ex.Message);
        }
    }
}