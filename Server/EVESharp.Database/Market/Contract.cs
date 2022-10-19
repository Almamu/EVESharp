namespace EVESharp.Database.Market;

public class Contract
{
    public int            ID            { get; init; }
    public double?        Price         { get; init; }
    public int            Collateral    { get; init; }
    public long           ExpireTime    { get; init; }
    public int            CrateID       { get; set; }
    public int            StationID     { get; init; }
    public ContractStatus Status        { get; set; }
    public ContractTypes  Type          { get; init; }
    public int            IssuerID      { get; init; }
    public int            IssuerCorpID  { get; init; }
    public bool           ForCorp       { get; init; }
    public double?        Reward        { get; init; }
    public double?        Volume        { get; set; }
    public int            AcceptorID    { get; set; }
    public long           AcceptedDate  { get; set; }
    public long           CompletedDate { get; set; }
}