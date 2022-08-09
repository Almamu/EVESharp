using System.Collections.Generic;
using EVESharp.EVE.Data.Dogma;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Attributes;

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