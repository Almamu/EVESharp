using EVE.Packets.Exceptions;

namespace Node.Exceptions.character
{
    class CharNameInvalidMaxSpaces : UserError
    {
        public CharNameInvalidMaxSpaces() : base("CharNameInvalidMaxSpaces")
        {
        }
    }
}