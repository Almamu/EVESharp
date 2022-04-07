﻿using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.insuranceSvc;

public class InsureShipFailed : UserError
{
    public InsureShipFailed (string reason) : base (
        "InsureShipFailed",
        new PyDictionary {["reason"] = reason}
    ) { }
}