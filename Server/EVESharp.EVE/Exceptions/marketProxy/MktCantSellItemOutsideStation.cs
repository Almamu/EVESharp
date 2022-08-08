using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.marketProxy;

public class MktCantSellItemOutsideStation : UserError
{
    public MktCantSellItemOutsideStation (long jumps) : base (
        "MktCantSellItemOutsideStation",
        new PyDictionary
        {
            ["numJumps"] = jumps,
            ["jumpText"] = "jump" + (jumps == 1 ? "" : "s")
        }
    ) { }
}