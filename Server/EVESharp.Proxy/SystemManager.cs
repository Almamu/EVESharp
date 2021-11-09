using System.Collections.Generic;
using System.Linq;
using EVESharp.Proxy.Database;

namespace EVESharp.Proxy
{
    public class SystemManager
    {
        private Dictionary<int, long> mLoadedSolarSystems = new Dictionary<int, long>();
        private ConnectionManager ConnectionManager { get; set; }
        private SolarSystemDB DB { get; }
        private GeneralDB GeneralDB { get; init; }

        public SystemManager(SolarSystemDB db, GeneralDB generalDB)
        {
            this.DB = db;
            this.GeneralDB = generalDB;
        }

        public void Init(ConnectionManager connectionManager)
        {
            this.ConnectionManager = connectionManager;
            this.DB.ClearSolarSystemNodeID();
            this.GeneralDB.ResetCharacterOnlineStatus();
        }
        
        public bool IsSolarSystemLoaded(int solarSystemID)
        {
            return this.mLoadedSolarSystems.ContainsKey(solarSystemID);
        }

        public long LoadSolarSystem(int solarSystemID)
        {
            if (this.IsSolarSystemLoaded(solarSystemID) == true)
                return this.mLoadedSolarSystems[solarSystemID];
            
            // sort nodes by amount of loaded solar systems and get the one with less solar systems in it
            List<NodeConnection> sortedList = this.ConnectionManager.Nodes.Values.OrderBy(x => x.SolarSystemLoadedCount).ToList();
            NodeConnection node = sortedList.First();

            node.SolarSystemLoadedCount++;

            this.MarkSolarSystemAsLoaded(solarSystemID, node.NodeID);
            
            return node.NodeID;
        }

        private void MarkSolarSystemAsLoaded(int solarSystemID, long nodeID)
        {
            this.mLoadedSolarSystems[solarSystemID] = nodeID;

            this.DB.SetSolarSystemNodeID(solarSystemID, nodeID);
        }
    }
}