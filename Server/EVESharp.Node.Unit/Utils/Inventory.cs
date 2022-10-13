using System.Collections.Generic;
using EVESharp.Database.Dogma;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory;

namespace EVESharp.Node.Unit.Utils;

public static class Inventory
{
    public static Type NewType(int typeID, string name = "No name")
    {
        return new Type (
            typeID, null, name, "", 0, 0, 0,
            0, 0, 0, 0, 0, true, 0, 0,
            new Dictionary <int, Attribute> (), new Dictionary <int, Effect> ()
        );
    }
}