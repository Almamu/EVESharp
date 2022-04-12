using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.corpStationMgr;

public class RentingOfficeRequestDenied : UserError
{
    public RentingOfficeRequestDenied (string reason) : base ("RentingOfficeRequestDenied", new PyDictionary {["reason"] = reason}) { }
}