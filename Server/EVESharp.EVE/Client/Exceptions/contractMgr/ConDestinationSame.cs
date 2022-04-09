using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConDestinationSame : UserError
{
    public ConDestinationSame () : base ("ConDestinationSame") { }
}