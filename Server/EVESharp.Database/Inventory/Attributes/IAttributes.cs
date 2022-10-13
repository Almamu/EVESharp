using System.Collections.Generic;

namespace EVESharp.Database.Inventory.Attributes;

public interface IAttributes : IReadOnlyDictionary <int, AttributeType>
{
    AttributeType this [AttributeTypes id] { get; }
}