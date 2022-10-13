namespace EVESharp.EVE.Data.Inventory;

public interface IBloodlines
{
    Bloodline this [int id] { get; }
}