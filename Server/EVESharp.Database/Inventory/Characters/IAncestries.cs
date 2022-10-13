namespace EVESharp.EVE.Data.Inventory;

public interface IAncestries
{
    Ancestry this [int id] { get; }
}