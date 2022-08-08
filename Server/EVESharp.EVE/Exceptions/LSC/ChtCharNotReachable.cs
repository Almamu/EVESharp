using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.LSC;

public class ChtCharNotReachable : UserError
{
    public ChtCharNotReachable (int characterID) : base ("ChtCharNotReachable", new PyDictionary {["char"] = FormatOwnerID (characterID)}) { }
}