using EVE.Packets.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConCannotTradeItemSanity : UserError
    {
        public ConCannotTradeItemSanity() : base("ConCannotTradeItemSanity")
        {
        }
    }
}