using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.repairSvc;

public class ConfirmRepackageSomethingWithUpgrades : UserError
{
    public ConfirmRepackageSomethingWithUpgrades () : base ("ConfirmRepackageSomethingWithUpgrades") { }
}