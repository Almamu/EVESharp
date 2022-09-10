using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.insuranceSvc;

public class InsureShipFailed : UserError
{
    public InsureShipFailed (string reason) : base (
        "InsureShipFailed",
        new PyDictionary {["reason"] = reason}
    ) { }
}