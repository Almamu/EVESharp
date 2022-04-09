using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class AllianceNameInvalidTaken : UserError
{
    public AllianceNameInvalidTaken () : base ("AllianceNameInvalidTaken") { }
}