﻿using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.corpStationMgr;

public class RentingYouHaveAnOfficeHere : UserError
{
    public RentingYouHaveAnOfficeHere() : base("RentingYouHaveAnOfficeHere")
    {
    }
}