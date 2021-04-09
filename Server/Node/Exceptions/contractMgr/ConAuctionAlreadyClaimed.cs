using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.contractMgr
{
    public class ConAuctionAlreadyClaimed : UserError
    {
        public ConAuctionAlreadyClaimed() : base("ConAuctionAlreadyClaimed")
        {
        }
    }
}