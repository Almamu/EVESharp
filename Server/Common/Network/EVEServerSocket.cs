using System;
using System.Net;
using System.Net.Sockets;

namespace Common.Network
{
    public class EVEServerSocket : EVESocket
    {
        public int Port { get; private set; }
        
        public EVEServerSocket(int port) : base()
        {
            this.Port = port;
        }

        public void Listen()
        {
            // bind the socket to the correct endpoint
            this.Socket.Bind(new IPEndPoint(IPAddress.Any, this.Port));
            this.Socket.Listen(20);
        }

        public void BeginAccept(AsyncCallback callback)
        {
            this.Socket.BeginAccept(callback, this);
        }

        public EVEClientSocket EndAccept(IAsyncResult asyncResult)
        {
            return new EVEClientSocket (this.Socket.EndAccept(asyncResult));
        }

        public override void GracefulDisconnect()
        {
            // graceful disconnect is the same as forcefully in a listening socket
            this.ForcefullyDisconnect();
        }
    }
}