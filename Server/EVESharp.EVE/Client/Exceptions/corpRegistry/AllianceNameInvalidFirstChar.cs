using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class AllianceNameInvalidFirstChar : UserError
{
    public AllianceNameInvalidFirstChar () : base ("AllianceNameInvalidFirstChar") { }
}