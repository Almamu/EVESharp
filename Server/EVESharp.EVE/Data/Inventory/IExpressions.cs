using System.Collections.Generic;
using EVESharp.EVE.Data.Dogma;

namespace EVESharp.EVE.Data.Inventory;

public interface IExpressions : IReadOnlyDictionary <int, Expression>
{
}