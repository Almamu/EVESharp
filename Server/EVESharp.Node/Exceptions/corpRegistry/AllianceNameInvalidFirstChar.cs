using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class AllianceNameInvalidFirstChar : UserError
{
    public AllianceNameInvalidFirstChar () : base ("AllianceNameInvalidFirstChar") { }
}