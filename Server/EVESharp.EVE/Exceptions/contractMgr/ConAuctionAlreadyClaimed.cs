using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConAuctionAlreadyClaimed : UserError
{
    public ConAuctionAlreadyClaimed () : base ("ConAuctionAlreadyClaimed") { }
}