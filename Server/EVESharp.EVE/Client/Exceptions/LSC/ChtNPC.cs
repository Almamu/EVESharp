using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.LSC;

public class ChtNPC : UserError
{
    public ChtNPC (int characterID) : base ("ChtNPC", new PyDictionary {["char"] = FormatOwnerID (characterID)}) { }
}