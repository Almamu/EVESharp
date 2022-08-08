using EVESharp.EVE.Data.Dogma;

namespace EVESharp.EVE.Data.Inventory;

public interface IExpressions
{
    Expression this [int index] { get; }
}