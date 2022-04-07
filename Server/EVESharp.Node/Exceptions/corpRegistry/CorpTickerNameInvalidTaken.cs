using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CorpTickerNameInvalidTaken : UserError
{
    public CorpTickerNameInvalidTaken () : base ("CorpTickerNameInvalidTaken") { }
}