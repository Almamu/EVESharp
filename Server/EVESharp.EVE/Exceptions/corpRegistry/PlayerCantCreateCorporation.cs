using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class PlayerCantCreateCorporation : UserError
{
    public PlayerCantCreateCorporation (int cost) : base ("PlayerCantCreateCorporation", new PyDictionary {["cost"] = FormatISK (cost)}) { }
}