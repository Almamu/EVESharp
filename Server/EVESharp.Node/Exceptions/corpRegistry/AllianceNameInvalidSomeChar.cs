using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class AllianceNameInvalidSomeChar : UserError
{
    public AllianceNameInvalidSomeChar() : base("AllianceNameInvalidSomeChar")
    {
            
    }
}