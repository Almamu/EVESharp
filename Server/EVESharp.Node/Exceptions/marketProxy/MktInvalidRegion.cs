using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.marketProxy;

public class MktInvalidRegion : UserError
{
    public MktInvalidRegion() : base("MktInvalidRegion")
    {
    }
}