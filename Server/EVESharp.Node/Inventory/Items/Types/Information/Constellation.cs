namespace EVESharp.Node.Inventory.Items.Types.Information;

public class Constellation
{
    public int    RegionId    { get; init; }
    public double X           { get; init; }
    public double Y           { get; init; }
    public double Z           { get; init; }
    public double XMin        { get; init; }
    public double YMin        { get; init; }
    public double ZMin        { get; init; }
    public double XMax        { get; init; }
    public double YMax        { get; init; }
    public double ZMax        { get; init; }
    public int?   FactionId   { get; init; }
    public double Radius      { get; init; }
    public Item   Information { get; init; }
}