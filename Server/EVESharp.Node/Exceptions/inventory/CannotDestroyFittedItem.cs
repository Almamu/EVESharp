using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.inventory;

public class CannotDestroyFittedItem : UserError
{
    public CannotDestroyFittedItem () : base ("CannotDestroyFittedItem") { }
}