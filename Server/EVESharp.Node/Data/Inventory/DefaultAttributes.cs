using System.Collections.Generic;
using System.Collections.ObjectModel;
using EVESharp.Database.Inventory;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Attributes;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Node.Data.Inventory;

public class DefaultAttributes : Dictionary <int, Dictionary <int, Attribute>>, IDefaultAttributes
{
    public DefaultAttributes (IDatabaseConnection Database, IAttributes attributes) : base (Database.InvDgmLoadDefaultAttributes (attributes))
    {
        
    }
}