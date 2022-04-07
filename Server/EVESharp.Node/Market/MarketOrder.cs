using EVESharp.Node.Database;

namespace EVESharp.Node.Market;

public class MarketOrder
{
    public int             OrderID       { get; }
    public int             TypeID        { get; }
    public int             CharacterID   { get; }
    public int             LocationID    { get; }
    public double          Price         { get; }
    public int             AccountID     { get; }
    public int             UnitsLeft     { get; }
    public int             MinimumUnits  { get; }
    public int             Range         { get; }
    public int             Jumps         { get; }
    public double          Escrow        { get; }
    public TransactionType Bid           { get; }
    public long            Issued        { get; }
    public int             CorporationID { get; }
    public bool            IsCorp        { get; }

    public MarketOrder (
        int  orderID,      int typeID, int characterID, int    locationID, double          price, int  accountID, int unitsLeft,
        int  minimumUnits, int range,  int jumps,       double escrow,     TransactionType bid,   long issued,    int corporationID,
        bool isCorp
    )
    {
        OrderID       = orderID;
        TypeID        = typeID;
        CharacterID   = characterID;
        LocationID    = locationID;
        Price         = price;
        AccountID     = accountID;
        UnitsLeft     = unitsLeft;
        MinimumUnits  = minimumUnits;
        Range         = range;
        Jumps         = jumps;
        Escrow        = escrow;
        Bid           = bid;
        Issued        = issued;
        CorporationID = corporationID;
        IsCorp        = isCorp;
    }
}