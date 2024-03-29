﻿using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.marketProxy;

public class RepairBeforeSelling : UserError
{
    public RepairBeforeSelling (Type type) : base (
        "RepairBeforeSelling", new PyDictionary
        {
            ["item"]      = FormatTypeIDAsName (type.ID),
            ["otheritem"] = FormatTypeIDAsName (type.ID)
        }
    ) { }
}