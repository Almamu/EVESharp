using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConContractNotOutstanding : UserError
{
    public ConContractNotOutstanding () : base ("ConContractNotOutstanding") { }
}