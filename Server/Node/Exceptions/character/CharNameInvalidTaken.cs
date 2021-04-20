using EVE.Packets.Exceptions;

namespace Node.Exceptions.character
{
    class CharNameInvalidTaken : UserError
    {
        public CharNameInvalidTaken() : base("CharNameInvalidTaken")
        {
        }
    }
}