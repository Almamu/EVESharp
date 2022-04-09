using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConCannotTradeCurrentShip : UserError
{
    public ConCannotTradeCurrentShip () : base ("ConCannotTradeCurrentShip") { }
}