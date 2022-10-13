namespace EVESharp.Database.Inventory.Categories;

public interface ICategories
{
    Category this [int id] { get; }
}