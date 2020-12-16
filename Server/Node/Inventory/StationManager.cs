using System.Collections.Generic;
using Node.Data;

namespace Node.Inventory
{
    public class StationManager
    {
        private ItemFactory mItemFactory;
        private Dictionary<int, StationOperations> mOperations;
        private Dictionary<int, StationType> mStationTypes;
        private Dictionary<int, string> mServices;
        public StationManager(ItemFactory factory)
        {
            this.mItemFactory = factory;
            this.mOperations = this.mItemFactory.StationDB.LoadOperations();
            this.mStationTypes = this.mItemFactory.StationDB.LoadStationTypes();
            this.mServices = this.mItemFactory.StationDB.LoadServices();
        }

        public Dictionary<int, StationOperations> Operations => this.mOperations;
        public Dictionary<int, StationType> StationTypes => this.mStationTypes;
        public Dictionary<int, string> Services => this.mServices;
    }
}