using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions;

public class ChtAlreadyInChannel : UserError
{
    public ChtAlreadyInChannel(int characterID) : base("ChtAlreadyInChannel", new PyDictionary {["char"] = FormatOwnerID(characterID)})
    {
    }
}