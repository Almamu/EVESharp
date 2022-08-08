using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConContractExpired : UserError
{
    public ConContractExpired () : base ("ConContractExpired") { }
}