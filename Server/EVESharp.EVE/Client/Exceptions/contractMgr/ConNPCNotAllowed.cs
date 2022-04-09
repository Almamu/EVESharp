using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConNPCNotAllowed : UserError
{
    public ConNPCNotAllowed () : base ("ConNPCNotAllowed") { }
}