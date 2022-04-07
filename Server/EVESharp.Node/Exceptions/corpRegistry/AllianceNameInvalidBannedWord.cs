using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class AllianceNameInvalidBannedWord : UserError
{
    public AllianceNameInvalidBannedWord() : base("AllianceNameInvalidBannedWord")
    {
            
    }
}