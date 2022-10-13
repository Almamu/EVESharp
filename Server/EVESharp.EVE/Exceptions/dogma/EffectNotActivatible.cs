using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.dogma;

public class EffectNotActivatible : UserError
{
    public EffectNotActivatible (Type type) : base ("EffectNotActivatible", new PyDictionary {["moduleName"] = FormatTypeIDAsName (type.ID)}) { }
}