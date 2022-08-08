using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConTooManyContractsMax : UserError
{
    public ConTooManyContractsMax (long maximumContracts) : base ("ConTooManyContractsMax", new PyDictionary {["max"] = FormatAmount (maximumContracts)}) { }
}