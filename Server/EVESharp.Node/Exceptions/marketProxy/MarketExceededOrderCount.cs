using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.marketProxy
{
    public class MarketExceededOrderCount : UserError
    {
        public MarketExceededOrderCount(int currentCount, int maximumCount) : base("MarketExceededOrderCount",
            new PyDictionary { ["curCnt"] = FormatAmount(currentCount), ["maxCnt"] = FormatAmount(maximumCount)})
        {
        }
    }
}