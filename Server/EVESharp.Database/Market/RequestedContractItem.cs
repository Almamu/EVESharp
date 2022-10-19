namespace EVESharp.Database.Market;

public struct RequestedContractItem
{
    public int    ItemID;
    public int    Quantity;
    public double Damage;
    public bool   Singleton;
    public bool   Contraband;
}