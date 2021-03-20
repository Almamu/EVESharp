using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.marketProxy
{
    public class MktCantSellItemOutsideStation : UserError
    {
        public MktCantSellItemOutsideStation(long jumps) : base("MktCantSellItemOutsideStation",
            new PyDictionary {["numJumps"] = jumps, ["jumpText"] = ("jump" + (jumps == 1 ? "" : "s"))})
        {
        }
    }
}