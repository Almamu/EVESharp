using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConTooManyContractsMax : UserError
{
    public ConTooManyContractsMax (long maximumContracts) : base ("ConTooManyContractsMax", new PyDictionary {["max"] = FormatAmount (maximumContracts)}) { }
}