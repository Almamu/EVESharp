using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class AllianceNameInvalidFirstChar : UserError
{
    public AllianceNameInvalidFirstChar () : base ("AllianceNameInvalidFirstChar") { }
}