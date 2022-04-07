using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.inventory;

public class NoFreeShipSlots : UserError
{
    public NoFreeShipSlots () : base ("NoFreeShipSlots") { }
}