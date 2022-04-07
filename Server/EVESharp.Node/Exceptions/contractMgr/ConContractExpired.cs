using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConContractExpired : UserError
{
    public ConContractExpired () : base ("ConContractExpired") { }
}