using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConBidTooLow : UserError
{
    public ConBidTooLow(int bid, int minBid) : base("ConBidTooLow", new PyDictionary {["bid"] = bid, ["minBid"] = minBid})
    {
    }
}