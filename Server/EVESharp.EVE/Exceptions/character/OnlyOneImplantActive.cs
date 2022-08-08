using EVESharp.EVE.Data.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.character;

public class OnlyOneImplantActive : UserError
{
    public OnlyOneImplantActive (Type implant) : base ("OnlyOneImplantActive", new PyDictionary {["typeName"] = FormatTypeIDAsName (implant.ID)}) { }
}