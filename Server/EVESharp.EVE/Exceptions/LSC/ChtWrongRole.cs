using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.LSC;

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