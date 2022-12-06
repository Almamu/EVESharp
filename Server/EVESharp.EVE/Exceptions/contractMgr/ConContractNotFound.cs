using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConContractNotFound : UserError
{
    public ConContractNotFound () : base ("ConContractNotFound") { }
}