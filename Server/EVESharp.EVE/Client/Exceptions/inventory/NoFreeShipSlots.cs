using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.inventory;

public class NoFreeShipSlots : UserError
{
    public NoFreeShipSlots () : base ("NoFreeShipSlots") { }
}