using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.inventory;

public class CannotMergeSingletonItems : UserError
{
    public CannotMergeSingletonItems () : base ("CannotMergeSingletonItems") { }
}