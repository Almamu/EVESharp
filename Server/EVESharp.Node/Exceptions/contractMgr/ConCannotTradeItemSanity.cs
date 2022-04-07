using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConCannotTradeItemSanity : UserError
{
    public ConCannotTradeItemSanity () : base ("ConCannotTradeItemSanity") { }
}