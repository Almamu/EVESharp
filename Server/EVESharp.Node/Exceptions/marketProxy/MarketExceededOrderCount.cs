using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.marketProxy;

public class MarketExceededOrderCount : UserError
{
    public MarketExceededOrderCount (int currentCount, int maximumCount) : base (
        "MarketExceededOrderCount",
        new PyDictionary
        {
            ["curCnt"] = FormatAmount (currentCount),
            ["maxCnt"] = FormatAmount (maximumCount)
        }
    ) { }
}