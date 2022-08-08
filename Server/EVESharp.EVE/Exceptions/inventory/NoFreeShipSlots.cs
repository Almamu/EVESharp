using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.inventory;

public class NoFreeShipSlots : UserError
{
    public NoFreeShipSlots () : base ("NoFreeShipSlots") { }
}