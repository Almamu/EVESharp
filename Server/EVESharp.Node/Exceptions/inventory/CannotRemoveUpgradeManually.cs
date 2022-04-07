using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.inventory;

public class CannotRemoveUpgradeManually : UserError
{
    public CannotRemoveUpgradeManually () : base ("CannotRemoveUpgradeManually") { }
}