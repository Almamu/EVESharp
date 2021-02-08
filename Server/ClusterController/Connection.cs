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

            if (data.birthday != Common.Constants.Game.birthday)
                throw new Exception("Wrong birthday in LowLevelVersionExchange");
            if (data.build != Common.Constants.Game.build)
                throw new Exception("Wrong build in LowLevelVersionExchange");
            if (data.codename != Common.Constants.Game.codename + "@" + Common.Constants.Game.region)
                throw new Exception("Wrong codename in LowLevelVersionExchange");
            if (data.machoVersion != Common.Constants.Game.machoVersion)
                throw new Exception("Wrong machoVersion in LowLevelVersionExchange");
            if (data.version != Common.Constants.Game.version)
                throw new Exception("Wrong version in LowLevelVersionExchange");
            if (data.isNode == true)
                if (data.nodeIdentifier != "Node")
                    throw new Exception("Wrong node string in LowLevelVersionExchange");

            return data;
        }
    }
}