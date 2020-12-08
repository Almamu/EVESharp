using System.Collections.Generic;
using Node.Data;

namespace Node.Inventory
{
    public class StationManager
    {
        private ItemFactory mItemFactory;
        private Dictionary<int, StationOperations> mOperations;
        private Dictionary<int, StationType> mStationTypes;
        public StationManager(ItemFactory factory)
        {
            this.mItemFactory = factory;
            this.mOperations = this.mItemFactory.StationDB.LoadOperations();
            this.mStationTypes = this.mItemFactory.StationDB.LoadStationTypes();
        }

        public Dictionary<int, StationOperations> Operations => this.mOperations;
        public Dictionary<int, StationType> StationTypes => this.mStationTypes;
    }
}