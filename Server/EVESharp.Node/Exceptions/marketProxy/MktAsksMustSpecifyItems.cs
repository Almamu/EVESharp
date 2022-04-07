using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.marketProxy;

public class MktAsksMustSpecifyItems : UserError
{
    public MktAsksMustSpecifyItems () : base ("MktAsksMustSpecifyItems") { }
}