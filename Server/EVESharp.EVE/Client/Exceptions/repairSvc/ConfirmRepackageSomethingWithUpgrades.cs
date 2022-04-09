using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.repairSvc;

public class ConfirmRepackageSomethingWithUpgrades : UserError
{
    public ConfirmRepackageSomethingWithUpgrades () : base ("ConfirmRepackageSomethingWithUpgrades") { }
}