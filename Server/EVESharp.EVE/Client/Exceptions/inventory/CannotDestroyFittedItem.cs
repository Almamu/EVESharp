using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.inventory;

public class CannotDestroyFittedItem : UserError
{
    public CannotDestroyFittedItem () : base ("CannotDestroyFittedItem") { }
}