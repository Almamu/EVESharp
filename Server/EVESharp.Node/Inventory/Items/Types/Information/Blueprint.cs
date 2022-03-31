namespace EVESharp.Node.Inventory.Items.Types.Information;

public class Blueprint
{
    public bool IsCopy { get; init; }
    public int MaterialLevel { get; init; }
    public int ProductivityLevel { get; init; }
    public int LicensedProductionRunsRemaining { get; init; }
    public Item Information { get; init; }
}