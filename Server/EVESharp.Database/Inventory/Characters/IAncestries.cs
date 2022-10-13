namespace EVESharp.Database.Inventory.Characters;

public interface IAncestries
{
    Ancestry this [int id] { get; }
}