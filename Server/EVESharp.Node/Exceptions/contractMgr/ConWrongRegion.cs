﻿using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConWrongRegion : UserError
{
    public ConWrongRegion () : base ("ConWrongRegion") { }
}