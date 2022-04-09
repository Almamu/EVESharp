using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class AllianceNameInvalidMinLength : UserError
{
    public AllianceNameInvalidMinLength () : base ("AllianceNameInvalidMinLength") { }
}