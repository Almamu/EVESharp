using System.Collections.Generic;

namespace EVESharp.EVE.Data.Inventory;

public interface ITypes : IReadOnlyDictionary <int, Type>
{
    Type this [TypeID      id] { get; }
    Type this [int        id] { get; }
    IEnumerable <int>                      Keys   { get; }
    IEnumerable <Type>                     Values { get; }
    int                                    Count  { get; }
    bool                                   ContainsKey (int typeID);
    bool                                   TryGetValue (int typeID, out Type value);
    IEnumerator <KeyValuePair <int, Type>> GetEnumerator ();
}