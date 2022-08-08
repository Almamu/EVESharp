namespace EVESharp.EVE.Data.Inventory.Items.Types.Information;

public class SolarSystem
{
    public int    RegionId        { get; init; }
    public int    ConstellationId { get; init; }
    public double MapX            { get; init; }
    public double MapY            { get; init; }
    public double MapZ            { get; init; }
    public double MapXMin         { get; init; }
    public double MapYMin         { get; init; }
    public double MapZMin         { get; init; }
    public double MapXMax         { get; init; }
    public double MapYMax         { get; init; }
    public double MapZMax         { get; init; }
    public double Luminosity      { get; init; }
    public bool   Border          { get; init; }
    public bool   Fringe          { get; init; }
    public bool   Corridor        { get; init; }
    public bool   Hub             { get; init; }
    public bool   International   { get; init; }
    public bool   Regional        { get; init; }
    public bool   Constellation   { get; init; }
    public double Security        { get; init; }
    public int?   FactionId       { get; init; }
    public double Radius          { get; init; }
    public int    SunTypeId       { get; init; }
    public string SecurityClass   { get; init; }
    public Item   Information     { get; init; }
}