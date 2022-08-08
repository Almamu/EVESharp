using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConCannotTradeCurrentShip : UserError
{
    public ConCannotTradeCurrentShip () : base ("ConCannotTradeCurrentShip") { }
}