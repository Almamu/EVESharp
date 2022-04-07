﻿using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.dogma;

public class EffectNotActivatible : UserError
{
    public EffectNotActivatible (Type type) : base ("EffectNotActivatible", new PyDictionary {["moduleName"] = FormatTypeIDAsName (type.ID)}) { }
}