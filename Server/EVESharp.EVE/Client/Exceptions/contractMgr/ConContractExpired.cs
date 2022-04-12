using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConContractExpired : UserError
{
    public ConContractExpired () : base ("ConContractExpired") { }
}