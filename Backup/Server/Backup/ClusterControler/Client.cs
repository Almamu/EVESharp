using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

using Common.Network;

namespace EVESharp.ClusterControler
{
    public class Client
    {
        private Socket socket = null;
        private AsyncCallback recvAsync = new AsyncCallback(ReceiveAsync);
        private AsyncCallback sendAsync = new AsyncCallback(SendAsync);
        private StreamPacketizer packetizer = new StreamPacketizer();

        public Client(Socket sock)
        {
            socket = sock;

            AsyncState state = new AsyncState();

            // Start receiving
            socket.BeginReceive(state.buffer, 0, 8192, SocketFlags.None, recvAsync, state);
        }

        public void ReceiveAsync(IAsyncResult ar)
        {
            AsyncState state = (AsyncState)(ar.AsyncState);

            int bytes = socket.EndReceive(ar);

            packetizer.QueuePackets(state.buffer, bytes);
            int p = packetizer.ProcessPackets();

            socket.BeginReceive(state.buffer, 0, 8192, SocketFlags.None, recvAsync, state);
        }

        public void SendAsync(IAsyncResult ar)
        {

        }

        // Getters and setters
        public int ClientID
        {
            get;
            set;
        }
    }
}
