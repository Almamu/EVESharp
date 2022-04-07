using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConNPCNotAllowed : UserError
{
    public ConNPCNotAllowed () : base ("ConNPCNotAllowed") { }
}