using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.inventory;

public class CannotRemoveUpgradeManually : UserError
{
    public CannotRemoveUpgradeManually () : base ("CannotRemoveUpgradeManually") { }
}