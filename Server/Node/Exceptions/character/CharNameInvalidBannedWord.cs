using EVE.Packets.Exceptions;

namespace Node.Exceptions.character
{
    class CharNameInvalidBannedWord : UserError
    {
        public CharNameInvalidBannedWord() : base("CharNameInvalidBannedWord")
        {
        }
    }
}