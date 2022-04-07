using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CanOnlyCreateCorpInStation : UserError
{
    public CanOnlyCreateCorpInStation () : base ("CanOnlyCreateCorpInStation") { }
}