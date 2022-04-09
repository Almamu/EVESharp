using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.LSC;

public class ChtWrongRole : UserError
{
    public ChtWrongRole (string channel, string missingRoles) :
        base (
            "ChtWrongRole", new PyDictionary
            {
                ["channel"]      = channel,
                ["missingroles"] = missingRoles
            }
        ) { }
}