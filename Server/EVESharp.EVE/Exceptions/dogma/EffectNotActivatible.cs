using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.dogma;

public class EffectNotActivatible : UserError
{
    public EffectNotActivatible (Type type) : base ("EffectNotActivatible", new PyDictionary {["moduleName"] = FormatTypeIDAsName (type.ID)}) { }
}