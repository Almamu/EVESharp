using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class AllianceNameInvalidMaxLength : UserError
{
    public AllianceNameInvalidMaxLength () : base ("AllianceNameInvalidMaxLength") { }
}