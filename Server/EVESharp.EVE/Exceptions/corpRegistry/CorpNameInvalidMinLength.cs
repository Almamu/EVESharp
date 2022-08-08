using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CorpNameInvalidMinLength : UserError
{
    public CorpNameInvalidMinLength () : base ("CorpNameInvalidMinLength") { }
}