using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.contractMgr
{
    public class ConCannotTradeCurrentShip : UserError
    {
        public ConCannotTradeCurrentShip() : base("ConCannotTradeCurrentShip")
        {
        }
    }
}