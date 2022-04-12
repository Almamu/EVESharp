using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class CorpNameInvalidMaxLength : UserError
{
    public CorpNameInvalidMaxLength () : base ("CorpNameInvalidMaxLength") { }
}