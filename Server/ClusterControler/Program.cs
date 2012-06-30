using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

using Common;
using Common.Network;

using Marshal;

namespace EVESharp.ClusterControler
{
    class Program
    {
        static TCPSocket socket = new TCPSocket(26000, false);
        static AsyncCallback acceptAsync = new AsyncCallback(AcceptAsync);

        static void AcceptAsync(IAsyncResult ar)
        {
            TCPSocket handler = (TCPSocket)(ar.AsyncState);

            Socket sock = handler.Socket.EndAccept(ar);

            Log.Debug("Cluster", "Incoming connection");

            AsyncState state = new AsyncState();

            handler.Socket.BeginAccept(acceptAsync, handler);

            ConnectionManager.AddConnection(sock);
        }

        static void Main(string[] args)
        {
            Log.Init("cluster", Log.LogLevel.All);

            Log.Info("Cluster", "Starting GameCluster");

            if (Database.Database.Init() == false)
            {
                Log.Error("Cluster", "Cannot connect to database");
                while (true) Thread.Sleep(1);
            }

            Log.Debug("Cluster", "Connected to database");

            if (socket.Listen(1) == false)
            {
                Log.Error("Cluster", "Cannot listen on port 26000");
                while (true) Thread.Sleep(1);
            }

            Log.Debug("Cluster", "Listening on port 26000");

            // Begin accept
            socket.Socket.BeginAccept(acceptAsync, socket);

            // Start the login queue
            LoginQueue.Start();

            while (true)
            {
                // Timers or whatever here? Or maybe async calls with timers? Pretty much that will help
                Thread.Sleep(1);
            }
        }
    }
}
