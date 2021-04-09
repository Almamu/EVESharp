using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.contractMgr
{
    public class ConBidTooLow : UserError
    {
        public ConBidTooLow(int bid, int minBid) : base("ConBidTooLow", new PyDictionary {["bid"] = bid, ["minBid"] = minBid})
        {
        }
    }
}