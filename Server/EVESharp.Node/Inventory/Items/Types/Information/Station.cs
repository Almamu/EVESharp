using EVESharp.Node.StaticData.Inventory.Station;

namespace EVESharp.Node.Inventory.Items.Types.Information;

public class Station
{
    public Type      Type                     { get; init; }
    public Operation Operations               { get; init; }
    public int       Security                 { get; init; }
    public double    DockingCostPerVolume     { get; init; }
    public double    MaxShipVolumeDockable    { get; init; }
    public int       OfficeRentalCost         { get; init; }
    public int       ConstellationID          { get; init; }
    public int       RegionID                 { get; init; }
    public double    ReprocessingEfficiency   { get; init; }
    public double    ReprocessingStationsTake { get; init; }
    public int       ReprocessingHangarFlag   { get; init; }
    public Item      Information              { get; init; }
}