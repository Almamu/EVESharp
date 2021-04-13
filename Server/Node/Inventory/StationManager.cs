using System.Collections.Generic;
using Node.Database;
using Node.StaticData;
using Node.StaticData.Inventory.Station;

namespace Node.Inventory
{
    public class StationManager
    {
        private StationDB StationDB { get; }
        private Dictionary<int, Operation> mOperations;
        private Dictionary<int, Type> mStationTypes;
        private Dictionary<int, string> mServices;
        public StationManager(StationDB stationDB)
        {
            this.StationDB = stationDB;
        }

        public void Load()
        {
            this.mOperations = this.StationDB.LoadOperations();
            this.mStationTypes = this.StationDB.LoadStationTypes();
            this.mServices = this.StationDB.LoadServices();
        }

        public Dictionary<int, Operation> Operations => this.mOperations;
        public Dictionary<int, Type> StationTypes => this.mStationTypes;
        public Dictionary<int, string> Services => this.mServices;
    }
}