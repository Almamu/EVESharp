using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.character;

public class OnlyOneImplantActive : UserError
{
    public OnlyOneImplantActive (Type implant) : base ("OnlyOneImplantActive", new PyDictionary {["typeName"] = FormatTypeIDAsName (implant.ID)}) { }
}