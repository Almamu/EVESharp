using System.Collections.Generic;

namespace EVESharp.EVE.Data.Inventory;

public interface IAttributes : IReadOnlyDictionary <int, AttributeType>
{
    AttributeType this [AttributeTypes id] { get; }
}