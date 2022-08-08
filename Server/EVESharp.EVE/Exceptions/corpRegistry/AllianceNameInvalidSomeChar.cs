using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class AllianceNameInvalidSomeChar : UserError
{
    public AllianceNameInvalidSomeChar () : base ("AllianceNameInvalidSomeChar") { }
}