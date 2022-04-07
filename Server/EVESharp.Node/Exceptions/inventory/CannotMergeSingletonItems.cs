using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.inventory;

public class CannotMergeSingletonItems : UserError
{
    public CannotMergeSingletonItems() : base("CannotMergeSingletonItems")
    {
    }
}