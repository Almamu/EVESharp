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

        protected LowLevelVersionExchange CheckLowLevelVersionExchange(PyDataType exchange)
        {
            LowLevelVersionExchange data = exchange;

            if (data.Birthday != Common.Constants.Game.BIRTHDAY)
                throw new Exception("Wrong birthday in LowLevelVersionExchange");
            if (data.Build != Common.Constants.Game.BUILD)
                throw new Exception("Wrong build in LowLevelVersionExchange");
            if (data.Codename != Common.Constants.Game.CODENAME + "@" + Common.Constants.Game.REGION)
                throw new Exception("Wrong codename in LowLevelVersionExchange");
            if (data.MachoVersion != Common.Constants.Game.MACHO_VERSION)
                throw new Exception("Wrong machoVersion in LowLevelVersionExchange");
            if (data.Version != Common.Constants.Game.VERSION)
                throw new Exception("Wrong version in LowLevelVersionExchange");
            if (data.IsNode == true)
                if (data.NodeIdentifier != "Node")
                    throw new Exception("Wrong node string in LowLevelVersionExchange");

            return data;
        }
    }
}