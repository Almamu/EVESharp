using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.LSC;

public class ChtAlreadyInChannel : UserError
{
    public ChtAlreadyInChannel (int characterID) : base ("ChtAlreadyInChannel", new PyDictionary {["char"] = FormatOwnerID (characterID)}) { }
}