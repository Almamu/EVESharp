using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class OnlyCEOCanPayoutDividends : UserError
{
    public OnlyCEOCanPayoutDividends () : base ("OnlyCEOCanPayoutDividends") { }
}