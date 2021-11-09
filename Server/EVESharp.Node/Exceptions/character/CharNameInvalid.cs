using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character
{
    class CharNameInvalid : UserError
    {
        public CharNameInvalid() : base("CharNameInvalid")
        {
        }
    }
}