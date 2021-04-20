using EVE.Packets.Exceptions;

namespace Node.Exceptions.character
{
    class CharNameInvalid : UserError
    {
        public CharNameInvalid() : base("CharNameInvalid")
        {
        }
    }
}