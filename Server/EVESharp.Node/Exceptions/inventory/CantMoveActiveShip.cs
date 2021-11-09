using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.inventory
{
    public class CantMoveActiveShip : UserError
    {
        public CantMoveActiveShip() : base("CantMoveActiveShip")
        {
        }
    }
}