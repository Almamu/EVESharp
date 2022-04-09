using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class CorpTickerNameInvalid : UserError
{
    public CorpTickerNameInvalid () : base ("CorpTickerNameInvalid") { }
}