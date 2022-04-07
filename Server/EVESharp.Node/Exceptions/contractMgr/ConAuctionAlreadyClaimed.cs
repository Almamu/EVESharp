using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConAuctionAlreadyClaimed : UserError
{
    public ConAuctionAlreadyClaimed() : base("ConAuctionAlreadyClaimed")
    {
    }
}