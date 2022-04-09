using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class CorpNameInvalidTaken : UserError
{
    public CorpNameInvalidTaken () : base ("CorpNameInvalidTaken") { }
}