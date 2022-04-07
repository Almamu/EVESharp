using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions;

public class ChtWrongRole : UserError
{
    public ChtWrongRole(string channel, string missingRoles) :
        base("ChtWrongRole", new PyDictionary {["channel"] = channel, ["missingroles"] = missingRoles})
    {
    }
}