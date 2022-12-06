using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConTooManyContracts : UserError
{
    public ConTooManyContracts (long maximumContracts) : base ("ConTooManyContracts", new PyDictionary {["max"] = FormatAmount (maximumContracts)}) { }
}