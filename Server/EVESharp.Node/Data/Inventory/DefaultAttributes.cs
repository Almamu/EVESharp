using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.Database.Extensions.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.EVE.Data.Inventory;

namespace EVESharp.Node.Data.Inventory;

public class DefaultAttributes : Dictionary <int, Dictionary <int, Attribute>>, IDefaultAttributes
{
    public DefaultAttributes (IDatabase Database, IAttributes attributes) : base (Database.InvDgmLoadDefaultAttributes (attributes)) { }
}