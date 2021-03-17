namespace Node.Market
{
    public class MarketOrder
    {
        public int OrderID { get; }
        public int TypeID { get; }
        public int ItemID { get; }
        public int CharacterID { get; }
        public int LocationID { get; }
        public double Price { get; }
        public int AccountID { get; }
        public int UnitsLeft { get; }
        public int MinimumUnits { get; }
        public int Range { get; }
        public int Jumps { get; }
        public double Escrow { get; }

        public MarketOrder(int orderID, int typeID, int itemID, int characterID, int locationID, double price, int accountID, int unitsLeft, int minimumUnits, int range, int jumps, double escrow)
        {
            this.OrderID = orderID;
            this.TypeID = typeID;
            this.ItemID = itemID;
            this.CharacterID = characterID;
            this.LocationID = locationID;
            this.Price = price;
            this.AccountID = accountID;
            this.UnitsLeft = unitsLeft;
            this.MinimumUnits = minimumUnits;
            this.Range = range;
            this.Jumps = jumps;
            this.Escrow = escrow;
        }
    }
}