using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CrpCantQuitNotInStasis : UserError
{
    // TODO: FILL THE ROLES TEXT WITH THE PROPER LIST
    public CrpCantQuitNotInStasis (int characterID, long roles) : base (
        "CrpCantQuitNotInStasis", new PyDictionary
        {
            ["charname"] = FormatOwnerID (characterID),
            ["rolelist"] = "some roles"
        }
    ) { }
}