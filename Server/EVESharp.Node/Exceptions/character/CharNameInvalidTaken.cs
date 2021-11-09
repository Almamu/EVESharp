using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character
{
    class CharNameInvalidTaken : UserError
    {
        public CharNameInvalidTaken() : base("CharNameInvalidTaken")
        {
        }
    }
}