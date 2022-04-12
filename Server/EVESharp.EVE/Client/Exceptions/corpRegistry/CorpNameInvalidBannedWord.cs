using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class CorpNameInvalidBannedWord : UserError
{
    public CorpNameInvalidBannedWord () : base ("CorpNameInvalidBannedWord") { }
}