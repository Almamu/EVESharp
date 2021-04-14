using System;
using Common.Network;
using Common.Packets;
using PythonTypes.Types.Primitives;

namespace ClusterController
{
    public abstract class Connection
    {
        public EVEClientSocket Socket { get; }
        protected ConnectionManager ConnectionManager { get; }

        public Connection(EVEClientSocket socket, ConnectionManager connectionManager)
        {
            this.Socket = socket;
            this.ConnectionManager = connectionManager;

            // set handler for connection lost events
            this.Socket.SetOnConnectionLostHandler(OnConnectionLost);
        }

        protected abstract void OnConnectionLost();
    }
}