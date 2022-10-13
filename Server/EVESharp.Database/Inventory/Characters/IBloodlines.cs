namespace EVESharp.Database.Inventory.Characters;

public interface IBloodlines
{
    Bloodline this [int id] { get; }
}