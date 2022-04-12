using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class CorpTickerNameInvalidTaken : UserError
{
    public CorpTickerNameInvalidTaken () : base ("CorpTickerNameInvalidTaken") { }
}