using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConAuctionAlreadyClaimed : UserError
{
    public ConAuctionAlreadyClaimed () : base ("ConAuctionAlreadyClaimed") { }
}