using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.inventory;

public class CannotMergeSingletonItems : UserError
{
    public CannotMergeSingletonItems () : base ("CannotMergeSingletonItems") { }
}