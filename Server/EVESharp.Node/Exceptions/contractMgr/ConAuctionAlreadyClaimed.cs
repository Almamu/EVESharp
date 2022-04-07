using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConAuctionAlreadyClaimed : UserError
{
    public ConAuctionAlreadyClaimed () : base ("ConAuctionAlreadyClaimed") { }
}