using System.Collections.Generic;
using Node.Data;
using Node.Database;

namespace Node.Inventory
{
    public class StationManager
    {
        private StationDB StationDB { get; }
        private Dictionary<int, StationOperations> mOperations;
        private Dictionary<int, StationType> mStationTypes;
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

        public Dictionary<int, StationOperations> Operations => this.mOperations;
        public Dictionary<int, StationType> StationTypes => this.mStationTypes;
        public Dictionary<int, string> Services => this.mServices;
    }
}