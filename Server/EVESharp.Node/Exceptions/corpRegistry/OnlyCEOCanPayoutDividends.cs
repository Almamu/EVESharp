using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class OnlyCEOCanPayoutDividends : UserError
{
    public OnlyCEOCanPayoutDividends () : base ("OnlyCEOCanPayoutDividends") { }
}