namespace EVESharp.EVE.Data.Inventory;

public interface ICategories
{
    Category this [int id] { get; }
}