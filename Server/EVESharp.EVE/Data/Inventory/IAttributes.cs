using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory.Attributes;

namespace EVESharp.EVE.Data.Inventory;

public interface IAttributes : IReadOnlyDictionary <int, AttributeType>
{
    AttributeType this [AttributeTypes id] { get; }
}