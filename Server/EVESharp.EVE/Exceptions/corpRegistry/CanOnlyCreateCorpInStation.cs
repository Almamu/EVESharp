using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CanOnlyCreateCorpInStation : UserError
{
    public CanOnlyCreateCorpInStation () : base ("CanOnlyCreateCorpInStation") { }
}