using EVE.Packets.Exceptions;

namespace Node.Exceptions.character
{
    class CharNameInvalidMaxLength : UserError
    {
        public CharNameInvalidMaxLength() : base("CharNameInvalidMaxLength")
        {
        }
    }
}