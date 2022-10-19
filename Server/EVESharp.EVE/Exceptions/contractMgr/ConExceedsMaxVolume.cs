using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConExceedsMaxVolume : UserError
{
    public ConExceedsMaxVolume (double vol, int max) : base (
        "ConExceedsMaxVolume", new PyDictionary
        {
            ["vol"]    = vol,
            ["max"] = max
        }
    ) { }
}