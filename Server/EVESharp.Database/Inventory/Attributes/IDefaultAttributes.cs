using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory.Attributes;

namespace EVESharp.EVE.Data.Inventory;

public interface IDefaultAttributes : IDictionary <int, Dictionary<int, Attribute>>
{
}