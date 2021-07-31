using EVE.Packets.Exceptions;

namespace Node.Exceptions.corpRegistry
{
    public class AllianceNameInvalidLastChar : UserError
    {
        public AllianceNameInvalidLastChar() : base("AllianceNameInvalidLastChar")
        {
            
        }
    }
}