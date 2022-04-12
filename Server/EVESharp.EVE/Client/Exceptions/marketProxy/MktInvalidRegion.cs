using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.marketProxy;

public class MktInvalidRegion : UserError
{
    public MktInvalidRegion () : base ("MktInvalidRegion") { }
}