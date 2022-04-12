using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConCannotTradeItemSanity : UserError
{
    public ConCannotTradeItemSanity () : base ("ConCannotTradeItemSanity") { }
}