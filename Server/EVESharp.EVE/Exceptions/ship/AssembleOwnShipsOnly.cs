using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.ship;

public class AssembleOwnShipsOnly : UserError
{
    public AssembleOwnShipsOnly (int ownerID) : base ("AssembleOwnShipsOnly", new PyDictionary {["owner"] = FormatOwnerID (ownerID)}) { }
}