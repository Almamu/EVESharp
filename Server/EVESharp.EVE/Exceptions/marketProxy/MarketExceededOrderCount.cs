using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.marketProxy;

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