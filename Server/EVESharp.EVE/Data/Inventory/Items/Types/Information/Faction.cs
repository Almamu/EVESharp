namespace EVESharp.EVE.Data.Inventory.Items.Types.Information;

public class Faction
{
    public string Description          { get; init; }
    public int    RaceIDs              { get; init; }
    public int    SolarSystemID        { get; init; }
    public int    CorporationID        { get; init; }
    public double SizeFactor           { get; init; }
    public int    StationCount         { get; init; }
    public int    StationSystemCount   { get; init; }
    public int    MilitiaCorporationID { get; init; }
    public Item   Information          { get; init; }
}