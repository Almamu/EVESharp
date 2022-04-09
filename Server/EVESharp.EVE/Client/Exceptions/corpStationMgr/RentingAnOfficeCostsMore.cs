using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.corpStationMgr;

public class RentingAnOfficeCostsMore : UserError
{
    public RentingAnOfficeCostsMore (int amount) : base ("RentingAnOfficeCostsMore", new PyDictionary {["amount"] = FormatISK (amount)}) { }
}