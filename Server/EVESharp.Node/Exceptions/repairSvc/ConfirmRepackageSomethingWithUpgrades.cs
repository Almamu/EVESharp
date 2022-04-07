using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.repairSvc;

public class ConfirmRepackageSomethingWithUpgrades : UserError
{
    public ConfirmRepackageSomethingWithUpgrades() : base("ConfirmRepackageSomethingWithUpgrades")
    {
    }
}