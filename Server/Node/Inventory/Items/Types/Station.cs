using System.Collections.Generic;
using System.Runtime.InteropServices;
using Node.Data;

namespace Node.Inventory.Items.Types
{
    public class Station : ItemInventory
    {
        private StationType mStationType;
        private StationOperations mOperations;
        private Dictionary<int, Character> mGuests;
        
        public Station(ItemEntity from, StationType stationType, StationOperations operations) : base(from)
        {
            this.mStationType = stationType;
            this.mOperations = operations;
            this.mGuests = new Dictionary<int, Character>();
        }

        public StationOperations Operations => this.mOperations;
        public StationType StationType => this.mStationType;
        public Dictionary<int, Character> Guests => this.mGuests;
    }
}