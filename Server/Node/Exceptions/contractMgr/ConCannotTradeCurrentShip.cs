using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConCannotTradeCurrentShip : UserError
    {
        public ConCannotTradeCurrentShip() : base("ConCannotTradeCurrentShip")
        {
        }
    }
}