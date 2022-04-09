using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.marketProxy;

public class MktOrderDidNotMatch : UserError
{
    public MktOrderDidNotMatch () : base ("MktOrderDidNotMatch") { }
}