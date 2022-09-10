using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.insuranceSvc;

public class InsureShipFailedSingleContract : UserError
{
    public InsureShipFailedSingleContract (int ownerID) : base (
        "InsureShipFailedSingleContract",
        new PyDictionary {["ownerName"] = FormatOwnerID (ownerID)}
    ) { }
}