namespace EVESharp.Database.Inventory.Groups;

public interface IGroups
{
    Group this [int id] { get; }
}