using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConMinMaxPriceError : UserError
{
    public ConMinMaxPriceError () : base ("ConMinMaxPriceError") { }
}