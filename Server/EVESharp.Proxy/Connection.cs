using System;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.EVE;
using EVESharp.EVE.Packets;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Proxy
{
    public abstract class Connection
    {
        public EVEClientSocket Socket { get; }
        protected ConnectionManager ConnectionManager { get; }
        protected Channel Log { get; }

        public Connection(EVEClientSocket socket, ConnectionManager connectionManager, Channel log)
        {
            this.Log = log;
            this.Socket = socket;
            this.Socket.Log = this.Log;
            this.ConnectionManager = connectionManager;

            // set handler for connection lost events
            this.Socket.SetOnConnectionLostHandler(OnConnectionLost);
        }

        protected void SendLowLevelVersionExchange()
        {
            Log.Debug("Sending LowLevelVersionExchange...");

            LowLevelVersionExchange data = new LowLevelVersionExchange
            {
                Codename = Game.CODENAME,
                Birthday = Game.BIRTHDAY,
                Build = Game.BUILD,
                MachoVersion = Game.MACHO_VERSION,
                Version = Game.VERSION,
                UserCount = this.ConnectionManager.ClientsCount,
                Region = Game.REGION
            };

            this.Socket.Send(data);
        }

        protected abstract void OnConnectionLost();
    }
}