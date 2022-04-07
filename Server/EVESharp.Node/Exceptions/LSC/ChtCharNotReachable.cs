using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions;

public class ChtCharNotReachable : UserError
{
    public ChtCharNotReachable(int characterID) : base("ChtCharNotReachable", new PyDictionary {["char"] = FormatOwnerID(characterID)})
    {
    }
}