using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class PlayerCantCreateCorporation : UserError
{
    public PlayerCantCreateCorporation (int cost) : base ("PlayerCantCreateCorporation", new PyDictionary {["cost"] = FormatISK (cost)}) { }
}