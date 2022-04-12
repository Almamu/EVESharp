namespace EVESharp.Node.Inventory.Items.Types.Information;

public class Corporation
{
    public string Description                { get; set; }
    public string TickerName                 { get; init; }
    public string Url                        { get; set; }
    public double TaxRate                    { get; set; }
    public double MinimumJoinStanding        { get; set; }
    public int    CorporationType            { get; init; }
    public bool   HasPlayerPersonnelManager  { get; set; }
    public bool   SendCharTerminationMessage { get; set; }
    public int    CreatorID                  { get; init; }
    public int    CeoID                      { get; set; }
    public int    StationID                  { get; set; }
    public int    RaceID                     { get; set; }
    public int?   AllianceID                 { get; set; }
    public long   Shares                     { get; set; }
    public int    MemberCount                { get; set; }
    public int    MemberLimit                { get; set; }
    public int    AllowedMemberRaceIDs       { get; set; }
    public int    GraphicId                  { get; set; }
    public int?   Shape1                     { get; set; }
    public int?   Shape2                     { get; set; }
    public int?   Shape3                     { get; set; }
    public int?   Color1                     { get; set; }
    public int?   Color2                     { get; set; }
    public int?   Color3                     { get; set; }
    public string Typeface                   { get; set; }
    public string Division1                  { get; set; }
    public string Division2                  { get; set; }
    public string Division3                  { get; set; }
    public string Division4                  { get; set; }
    public string Division5                  { get; set; }
    public string Division6                  { get; set; }
    public string Division7                  { get; set; }
    public string WalletDivision1            { get; set; }
    public string WalletDivision2            { get; set; }
    public string WalletDivision3            { get; set; }
    public string WalletDivision4            { get; set; }
    public string WalletDivision5            { get; set; }
    public string WalletDivision6            { get; set; }
    public string WalletDivision7            { get; set; }
    public bool   Deleted                    { get; set; }
    public long?  StartDate                  { get; set; }
    public int?   ExecutorCorpID             { get; set; }
    public Item   Information                { get; init; }
}