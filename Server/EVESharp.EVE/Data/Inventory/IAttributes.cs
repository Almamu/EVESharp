using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory.Attributes;

namespace EVESharp.EVE.Data.Inventory;

public interface IAttributes
{
    Dictionary <int, Dictionary <int, Attribute>> DefaultAttributes { get; }
    AttributeType this [int id] { get; }
    AttributeType this [AttributeTypes id] { get; }
}