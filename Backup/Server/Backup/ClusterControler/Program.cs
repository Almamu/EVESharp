using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

using Common;
using Common.Network;

namespace EVESharp.ClusterControler
{
    class Program
    {
        static TCPSocket socket = new TCPSocket(5000, false);
        static AsyncCallback acceptAsync = new AsyncCallback(AcceptAsync);

        static void AcceptAsync(IAsyncResult ar)
        {
            TCPSocket handler = (TCPSocket)(ar.AsyncState);

            Socket sock = handler.Socket.EndAccept(ar);

            AsyncState state = new AsyncState();


            // sock.BeginReceive(state.buffer, 0, 8192, SocketFlags.None, recvAsync, state);
            handler.Socket.BeginAccept(acceptAsync, handler);

            ClientManager.AddClient(sock);
        }

        static void Main(string[] args)
        {
            Log.Init("cluster", Log.LogLevel.All);

            Log.Info("Cluster", "Starting GameCluster");

            if (socket.Listen(1))
            {
                Log.Error("Cluster", "Cannot listen on port 5000");
                while (true) Thread.Sleep(1);
            }

            // Begin accept
            socket.Socket.BeginAccept(acceptAsync, socket);

            while (true)
            {
                Thread.Sleep(1);
            }
        }
    }
}
