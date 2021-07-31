using EVE.Packets.Exceptions;

namespace Node.Exceptions.corpRegistry
{
    public class AllianceNameInvalidFirstChar : UserError
    {
        public AllianceNameInvalidFirstChar() : base("AllianceNameInvalidFirstChar")
        {
            
        }
    }
}