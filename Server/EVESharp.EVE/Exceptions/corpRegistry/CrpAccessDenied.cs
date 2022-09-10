using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CrpAccessDenied : UserError
{
    public CrpAccessDenied (string reason) : base ("CrpAccessDenied", new PyDictionary {["reason"] = reason}) { }
}