using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConDestinationSame : UserError
{
    public ConDestinationSame () : base ("ConDestinationSame") { }
}