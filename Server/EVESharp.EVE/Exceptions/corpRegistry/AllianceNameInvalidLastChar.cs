using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class AllianceNameInvalidLastChar : UserError
{
    public AllianceNameInvalidLastChar () : base ("AllianceNameInvalidLastChar") { }
}