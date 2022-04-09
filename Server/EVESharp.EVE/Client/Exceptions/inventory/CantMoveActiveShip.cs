using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.inventory;

public class CantMoveActiveShip : UserError
{
    public CantMoveActiveShip () : base ("CantMoveActiveShip") { }
}