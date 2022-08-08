using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.inventory;

public class CantMoveActiveShip : UserError
{
    public CantMoveActiveShip () : base ("CantMoveActiveShip") { }
}