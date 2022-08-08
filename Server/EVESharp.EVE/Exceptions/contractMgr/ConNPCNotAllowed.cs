using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConNPCNotAllowed : UserError
{
    public ConNPCNotAllowed () : base ("ConNPCNotAllowed") { }
}