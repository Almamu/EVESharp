using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class AllianceNameInvalidLastChar : UserError
{
    public AllianceNameInvalidLastChar () : base ("AllianceNameInvalidLastChar") { }
}