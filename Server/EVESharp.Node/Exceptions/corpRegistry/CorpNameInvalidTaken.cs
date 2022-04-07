using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CorpNameInvalidTaken : UserError
{
    public CorpNameInvalidTaken () : base ("CorpNameInvalidTaken") { }
}