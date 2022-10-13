using EVESharp.Database.Inventory.Attributes;

namespace EVESharp.Database.Inventory.Types.Information;

/// <summary>
/// Represents the basic item information
/// </summary>
public class Item
{
    public int           ID         { get; init; }
    public string        Name       { get; set; }
    public Type          Type       { get; init; }
    public int           OwnerID    { get; set; }
    public int           LocationID { get; set; }
    public Flags         Flag       { get; set; }
    public bool          Contraband { get; set; }
    public bool          Singleton  { get; set; }
    public int           Quantity   { get; set; }
    public double?       X          { get; set; }
    public double?       Y          { get; set; }
    public double?       Z          { get; set; }
    public string        CustomInfo { get; set; }
    public AttributeList Attributes { get; init; }
    public bool          Dirty      { get; set; } = false;
    public bool          New        { get; set; } = false;
}