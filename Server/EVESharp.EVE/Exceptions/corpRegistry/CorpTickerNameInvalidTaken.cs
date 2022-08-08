using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CorpTickerNameInvalidTaken : UserError
{
    public CorpTickerNameInvalidTaken () : base ("CorpTickerNameInvalidTaken") { }
}