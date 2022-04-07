using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConDestinationSame : UserError
{
    public ConDestinationSame () : base ("ConDestinationSame") { }
}