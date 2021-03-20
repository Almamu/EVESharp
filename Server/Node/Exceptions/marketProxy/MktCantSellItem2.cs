﻿using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.marketProxy
{
    public class MktCantSellItem2 : UserError
    {
        public MktCantSellItem2(long jumps, long maximumJumps) : base("MktCantSellItem2",
            new PyDictionary
            {
                ["numJumps"] = jumps, ["jumpText"] = ("jump" + (jumps == 1 ? "" : "s")),
                ["numLimit"] = maximumJumps, ["jumpText2"] = ("jump" + (maximumJumps == 1 ? "" : "s"))
            })
        {
        }
    }
}