using System.Collections.Generic;

namespace EVESharp.EVE.Data.Inventory;

public interface ITypes : IReadOnlyDictionary <int, Type>
{
    Type this [TypeID      id] { get; }
}