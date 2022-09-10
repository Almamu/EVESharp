using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.corporationSvc;

public class ConfirmGivingMedal : UserError
{
    public ConfirmGivingMedal (int cost) : base ("ConfirmGivingMedal", new PyDictionary {["cost"] = cost}) { }
}