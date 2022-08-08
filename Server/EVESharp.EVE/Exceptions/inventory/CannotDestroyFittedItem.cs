using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.inventory;

public class CannotDestroyFittedItem : UserError
{
    public CannotDestroyFittedItem () : base ("CannotDestroyFittedItem") { }
}