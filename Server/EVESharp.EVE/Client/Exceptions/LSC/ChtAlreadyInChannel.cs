using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.LSC;

public class ChtAlreadyInChannel : UserError
{
    public ChtAlreadyInChannel (int characterID) : base ("ChtAlreadyInChannel", new PyDictionary {["char"] = FormatOwnerID (characterID)}) { }
}