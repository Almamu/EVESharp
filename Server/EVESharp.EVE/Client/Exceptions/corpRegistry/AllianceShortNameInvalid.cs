using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class AllianceShortNameInvalid : UserError
{
    public AllianceShortNameInvalid () : base ("AllianceShortNameInvalid") { }
}