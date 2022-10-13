using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.character;

public class PrereqImplantMissing : UserError
{
    public PrereqImplantMissing (int typeID) : base ("PrereqImplantMissing", new PyDictionary {["typeName"] = FormatTypeIDAsName (typeID)}) { }

    public PrereqImplantMissing (Type type) : this (type.ID) { }
}