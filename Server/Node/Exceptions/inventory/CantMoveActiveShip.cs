using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.inventory
{
    public class CantMoveActiveShip : UserError
    {
        public CantMoveActiveShip() : base("CantMoveActiveShip")
        {
        }
    }
}