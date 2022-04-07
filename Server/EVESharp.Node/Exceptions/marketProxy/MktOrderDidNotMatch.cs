using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.marketProxy;

public class MktOrderDidNotMatch : UserError
{
    public MktOrderDidNotMatch () : base ("MktOrderDidNotMatch") { }
}