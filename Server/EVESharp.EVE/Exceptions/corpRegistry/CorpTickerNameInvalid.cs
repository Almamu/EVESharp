using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CorpTickerNameInvalid : UserError
{
    public CorpTickerNameInvalid () : base ("CorpTickerNameInvalid") { }
}