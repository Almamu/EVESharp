using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConCannotTradeCurrentShip : UserError
{
    public ConCannotTradeCurrentShip () : base ("ConCannotTradeCurrentShip") { }
}