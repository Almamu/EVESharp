﻿using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.repairSvc;

public class ConfirmRepackageSomethingWithUpgrades : UserError
{
    public ConfirmRepackageSomethingWithUpgrades () : base ("ConfirmRepackageSomethingWithUpgrades") { }
}