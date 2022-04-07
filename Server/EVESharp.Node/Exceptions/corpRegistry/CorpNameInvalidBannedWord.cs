using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CorpNameInvalidBannedWord : UserError
{
    public CorpNameInvalidBannedWord () : base ("CorpNameInvalidBannedWord") { }
}