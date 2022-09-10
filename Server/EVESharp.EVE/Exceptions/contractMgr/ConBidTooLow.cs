using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConBidTooLow : UserError
{
    public ConBidTooLow (int bid, int minBid) : base (
        "ConBidTooLow", new PyDictionary
        {
            ["bid"]    = bid,
            ["minBid"] = minBid
        }
    ) { }
}