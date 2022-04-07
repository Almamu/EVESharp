using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CorpNameInvalidMinLength : UserError
{
    public CorpNameInvalidMinLength () : base ("CorpNameInvalidMinLength") { }
}