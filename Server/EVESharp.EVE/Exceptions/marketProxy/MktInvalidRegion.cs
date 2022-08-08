using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.marketProxy;

public class MktInvalidRegion : UserError
{
    public MktInvalidRegion () : base ("MktInvalidRegion") { }
}