using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CorpNameInvalidMaxLength : UserError
{
    public CorpNameInvalidMaxLength () : base ("CorpNameInvalidMaxLength") { }
}