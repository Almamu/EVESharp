using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.dogma;

public class EffectNotActivatible : UserError
{
    public EffectNotActivatible (Type type) : base ("EffectNotActivatible", new PyDictionary {["moduleName"] = FormatTypeIDAsName (type.ID)}) { }
}