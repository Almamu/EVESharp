using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class AllianceNameInvalidLastChar : UserError
{
    public AllianceNameInvalidLastChar () : base ("AllianceNameInvalidLastChar") { }
}