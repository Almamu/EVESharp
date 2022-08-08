using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.marketProxy;

public class MktAsksMustSpecifyItems : UserError
{
    public MktAsksMustSpecifyItems () : base ("MktAsksMustSpecifyItems") { }
}