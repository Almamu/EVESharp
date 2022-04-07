using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.character;

public class PrereqImplantMissing : UserError
{
    public PrereqImplantMissing (int typeID) : base ("PrereqImplantMissing", new PyDictionary {["typeName"] = FormatTypeIDAsName (typeID)}) { }

    public PrereqImplantMissing (Type type) : this (type.ID) { }
}