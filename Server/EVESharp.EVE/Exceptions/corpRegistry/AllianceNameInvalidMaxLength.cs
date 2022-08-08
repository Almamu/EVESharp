using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class AllianceNameInvalidMaxLength : UserError
{
    public AllianceNameInvalidMaxLength () : base ("AllianceNameInvalidMaxLength") { }
}