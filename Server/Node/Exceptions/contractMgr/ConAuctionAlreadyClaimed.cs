using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.contractMgr
{
    public class ConAuctionAlreadyClaimed : UserError
    {
        public ConAuctionAlreadyClaimed() : base("ConAuctionAlreadyClaimed")
        {
        }
    }
}