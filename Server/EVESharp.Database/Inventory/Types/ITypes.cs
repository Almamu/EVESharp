using System.Collections.Generic;

namespace EVESharp.Database.Inventory.Types;

public interface ITypes : IReadOnlyDictionary <int, Type>
{
    Type this [TypeID      id] { get; }
}