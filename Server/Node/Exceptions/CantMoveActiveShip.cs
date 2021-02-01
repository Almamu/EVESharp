using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class CantMoveActiveShip : UserError
    {
        public CantMoveActiveShip() : base("CantMoveActiveShip")
        {
        }
    }
}