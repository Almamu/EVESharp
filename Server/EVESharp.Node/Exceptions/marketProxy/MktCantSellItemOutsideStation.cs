using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.marketProxy;

public class MktCantSellItemOutsideStation : UserError
{
    public MktCantSellItemOutsideStation(long jumps) : base("MktCantSellItemOutsideStation",
                                                            new PyDictionary {["numJumps"] = jumps, ["jumpText"] = ("jump" + (jumps == 1 ? "" : "s"))})
    {
    }
}