using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class AllianceNameInvalidMinLength : UserError
{
    public AllianceNameInvalidMinLength() : base("AllianceNameInvalidMinLength")
    {
            
    }
}