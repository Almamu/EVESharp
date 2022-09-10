using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.LSC;

public class ChtNPC : UserError
{
    public ChtNPC (int characterID) : base ("ChtNPC", new PyDictionary {["char"] = FormatOwnerID (characterID)}) { }
}