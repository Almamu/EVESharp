using EVESharp.EVE.Data.Inventory;

namespace EVESharp.Node.Inventory;

public interface IGroups
{
    Group this [int id] { get; }
}