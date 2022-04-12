using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConContractNotOutstanding : UserError
{
    public ConContractNotOutstanding () : base ("ConContractNotOutstanding") { }
}