using Node.Data;

namespace Node.Inventory.Items.Types
{
    public class Station : ItemInventory
    {
        private StationType mStationType;
        private StationOperations mOperations;
        
        public Station(ItemEntity from, StationType stationType, StationOperations operations) : base(from)
        {
            this.mStationType = stationType;
            this.mOperations = operations;
        }

        public StationOperations Operations => this.mOperations;
        public StationType StationType => this.mStationType;
    }
}