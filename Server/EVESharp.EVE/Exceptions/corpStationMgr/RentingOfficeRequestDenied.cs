using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.corpStationMgr;

public class RentingOfficeRequestDenied : UserError
{
    public RentingOfficeRequestDenied (string reason) : base ("RentingOfficeRequestDenied", new PyDictionary {["reason"] = reason}) { }
}