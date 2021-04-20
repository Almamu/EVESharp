using EVE.Packets.Exceptions;

namespace Node.Exceptions.character
{
    class CharNameInvalidMinLength : UserError
    {
        public CharNameInvalidMinLength() : base("CharNameInvalidMinLength")
        {
        }
    }
}