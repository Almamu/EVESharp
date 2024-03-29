﻿using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CrpCantQuitNotCompletedStasisPeriodIsBlocked : UserError
{
    public CrpCantQuitNotCompletedStasisPeriodIsBlocked (int characterID, int hours, int hoursleft) : base (
        "CrpCantQuitNotCompletedStasisPeriodIsBlocked",
        new PyDictionary
        {
            ["charname"]  = FormatOwnerID (characterID),
            ["hour"]      = hours,
            ["hoursleft"] = hoursleft
        }
    ) { }
}