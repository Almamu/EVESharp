﻿using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class AllianceShortNameInvalidTaken : UserError
{
    public AllianceShortNameInvalidTaken () : base ("AllianceShortNameInvalidTaken") { }
}