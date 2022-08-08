using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CorpNameInvalidMaxLength : UserError
{
    public CorpNameInvalidMaxLength () : base ("CorpNameInvalidMaxLength") { }
}