using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class AllianceNameInvalidSomeChar : UserError
{
    public AllianceNameInvalidSomeChar () : base ("AllianceNameInvalidSomeChar") { }
}