using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CorpTickerNameInvalid : UserError
{
    public CorpTickerNameInvalid () : base ("CorpTickerNameInvalid") { }
}