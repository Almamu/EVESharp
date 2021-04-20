using EVE.Packets.Exceptions;

namespace Node.Exceptions.inventory
{
    public class CantMoveActiveShip : UserError
    {
        public CantMoveActiveShip() : base("CantMoveActiveShip")
        {
        }
    }
}