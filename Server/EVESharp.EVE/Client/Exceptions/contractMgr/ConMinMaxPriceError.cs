using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConMinMaxPriceError : UserError
{
    public ConMinMaxPriceError () : base ("ConMinMaxPriceError") { }
}