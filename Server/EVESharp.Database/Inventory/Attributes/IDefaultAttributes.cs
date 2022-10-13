using System.Collections.Generic;

namespace EVESharp.Database.Inventory.Attributes;

public interface IDefaultAttributes : IDictionary <int, Dictionary<int, Attribute>>
{
}